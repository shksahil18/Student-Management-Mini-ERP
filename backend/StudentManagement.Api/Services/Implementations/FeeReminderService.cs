using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudentManagement.Api.Data;
using StudentManagement.Api.Entities;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Services.Implementations;

public class FeeReminderService : IFeeReminderService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly EmailOptions _options;
    private readonly ILogger<FeeReminderService> _logger;

    public FeeReminderService(
        ApplicationDbContext context,
        IEmailService emailService,
        IEmailTemplateService emailTemplateService,
        IOptions<EmailOptions> options,
        ILogger<FeeReminderService> logger)
    {
        _context = context;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> ProcessUpcomingAndOverdueRemindersAsync(CancellationToken cancellationToken = default)
    {
        var processedCount = 0;
        var now = DateTime.UtcNow;

        // 1. Query all active students with outstanding fee balances
        var outstandingFees = await _context.Fees
            .Include(f => f.Student)
            .Where(f => f.OutstandingAmount > 0 &&
                        f.Status != "Paid" &&
                        f.Student.IsActive &&
                        !string.IsNullOrWhiteSpace(f.Student.Email))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Checking {Count} outstanding fee records for reminder eligibility...", outstandingFees.Count);

        foreach (var fee in outstandingFees)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var dueDate = fee.DueDate.Date;
            var today = now.Date;
            var daysDiff = (dueDate - today).Days;

            // Scenario A: Upcoming Reminder (e.g. exactly <= LeadDays away and > 0 days)
            if (daysDiff >= 0 && daysDiff <= _options.UpcomingReminderLeadDays)
            {
                // Check if an upcoming reminder was already sent for this fee within the last lead window
                var alreadySentUpcoming = await _context.FeeReminderLogs
                    .AnyAsync(r => r.FeeId == fee.Id &&
                                   r.ReminderType == ReminderType.Upcoming &&
                                   r.SentAt >= now.AddDays(-_options.UpcomingReminderLeadDays),
                              cancellationToken);

                if (!alreadySentUpcoming)
                {
                    var emailMsg = _emailTemplateService.BuildUpcomingFeeReminderEmail(fee.Student, fee, Math.Max(daysDiff, 1));
                    await _emailService.QueueEmailAsync(emailMsg, cancellationToken);

                    var log = new FeeReminderLog
                    {
                        FeeId = fee.Id,
                        StudentId = fee.StudentId,
                        ReminderType = ReminderType.Upcoming,
                        OutstandingAmount = fee.OutstandingAmount,
                        RecipientEmail = fee.Student.Email,
                        SentAt = now,
                        IsSuccess = true
                    };
                    await _context.FeeReminderLogs.AddAsync(log, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    processedCount++;
                    _logger.LogInformation("Upcoming reminder queued for Student {Code}, Fee #{FeeId}", fee.Student.StudentCode, fee.Id);
                }
            }
            // Scenario B: Overdue Reminder (e.g. daysDiff < 0)
            else if (daysDiff < 0)
            {
                var daysOverdue = Math.Abs(daysDiff);

                // Enforce configurable cooldown period (e.g. 7 days) to prevent spamming
                var recentOverdueLog = await _context.FeeReminderLogs
                    .Where(r => r.FeeId == fee.Id &&
                                r.ReminderType == ReminderType.Overdue &&
                                r.SentAt >= now.AddDays(-_options.OverdueReminderCooldownDays))
                    .FirstOrDefaultAsync(cancellationToken);

                if (recentOverdueLog == null)
                {
                    var emailMsg = _emailTemplateService.BuildOverdueFeeReminderEmail(fee.Student, fee, daysOverdue);
                    await _emailService.QueueEmailAsync(emailMsg, cancellationToken);

                    var log = new FeeReminderLog
                    {
                        FeeId = fee.Id,
                        StudentId = fee.StudentId,
                        ReminderType = ReminderType.Overdue,
                        OutstandingAmount = fee.OutstandingAmount,
                        RecipientEmail = fee.Student.Email,
                        SentAt = now,
                        IsSuccess = true
                    };
                    await _context.FeeReminderLogs.AddAsync(log, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    processedCount++;
                    _logger.LogInformation("Overdue reminder queued for Student {Code}, Fee #{FeeId} ({Days} days overdue)",
                        fee.Student.StudentCode, fee.Id, daysOverdue);
                }
            }
        }

        return processedCount;
    }
}

