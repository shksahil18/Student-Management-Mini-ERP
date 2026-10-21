using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using StudentManagement.Api.Data;
using StudentManagement.Api.Entities;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Services.Implementations;

public class SmtpEmailService : IEmailService
{
    private readonly ApplicationDbContext _context;
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        ApplicationDbContext context,
        IOptions<EmailOptions> options,
        ILogger<SmtpEmailService> logger)
    {
        _context = context;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailDirectAsync(EmailMessageDto message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) || _options.SmtpHost == "localhost" && string.IsNullOrWhiteSpace(_options.SmtpUsername))
        {
            _logger.LogWarning("SMTP is not configured with credentials (Host: {Host}:{Port}). Simulating safe log delivery for recipient: {ToEmail}",
                _options.SmtpHost, _options.SmtpPort, message.ToEmail);
            return true; // Safe fallback when SMTP credentials are not yet configured in local environment
        }

        try
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
            mimeMessage.To.Add(new MailboxAddress(message.ToName, message.ToEmail));
            mimeMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message.HtmlBody,
                TextBody = message.PlainTextBody
            };

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 15000; // 15-second timeout safeguard

            var secureSocketOptions = _options.EnableSsl
                ? SecureSocketOptions.Auto
                : SecureSocketOptions.None;

            await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.SmtpUsername) && !string.IsNullOrWhiteSpace(_options.SmtpPassword))
            {
                await client.AuthenticateAsync(_options.SmtpUsername, _options.SmtpPassword, cancellationToken);
            }

            await client.SendAsync(mimeMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email [{EmailType}] successfully dispatched to {ToEmail}", message.EmailType, message.ToEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email [{EmailType}] to {ToEmail}: {Error}", message.EmailType, message.ToEmail, ex.Message);
            return false;
        }
    }

    public async Task QueueEmailAsync(EmailMessageDto message, CancellationToken cancellationToken = default)
    {
        // 1. Idempotency check: prevent duplicate outbox entries for the same reference
        if (!string.IsNullOrWhiteSpace(message.ReferenceId))
        {
            var exists = await _context.EmailOutbox
                .AnyAsync(o => o.EmailType == message.EmailType &&
                               o.ReferenceId == message.ReferenceId &&
                               (o.Status == EmailOutboxStatus.Sent || o.Status == EmailOutboxStatus.Pending || o.Status == EmailOutboxStatus.Processing),
                          cancellationToken);

            if (exists)
            {
                _logger.LogInformation("Email [{EmailType}] with ReferenceId '{ReferenceId}' is already in Outbox. Suppressing duplicate queuing.",
                    message.EmailType, message.ReferenceId);
                return;
            }
        }

        var outboxItem = new EmailOutbox
        {
            ToEmail = message.ToEmail,
            ToName = message.ToName,
            Subject = message.Subject,
            HtmlBody = message.HtmlBody,
            PlainTextBody = message.PlainTextBody,
            EmailType = message.EmailType,
            ReferenceId = message.ReferenceId,
            Status = EmailOutboxStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _context.EmailOutbox.AddAsync(outboxItem, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Queued email [{EmailType}] for {ToEmail} (Outbox ID: {Id})", outboxItem.EmailType, outboxItem.ToEmail, outboxItem.Id);
    }

    public async Task ProcessPendingOutboxAsync(int batchSize = 10, CancellationToken cancellationToken = default)
    {
        var pendingItems = await _context.EmailOutbox
            .Where(o => (o.Status == EmailOutboxStatus.Pending || (o.Status == EmailOutboxStatus.Failed && o.RetryCount < o.MaxRetries)))
            .OrderBy(o => o.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (!pendingItems.Any()) return;

        foreach (var item in pendingItems)
        {
            if (cancellationToken.IsCancellationRequested) break;

            item.Status = EmailOutboxStatus.Processing;
            await _context.SaveChangesAsync(cancellationToken);

            var messageDto = new EmailMessageDto
            {
                ToEmail = item.ToEmail,
                ToName = item.ToName,
                Subject = item.Subject,
                HtmlBody = item.HtmlBody,
                PlainTextBody = item.PlainTextBody,
                EmailType = item.EmailType,
                ReferenceId = item.ReferenceId
            };

            var success = await SendEmailDirectAsync(messageDto, cancellationToken);
            if (success)
            {
                item.Status = EmailOutboxStatus.Sent;
                item.ProcessedAt = DateTime.UtcNow;
                item.ErrorMessage = null;
            }
            else
            {
                item.RetryCount++;
                item.Status = item.RetryCount >= item.MaxRetries ? EmailOutboxStatus.Failed : EmailOutboxStatus.Pending;
                item.ErrorMessage = $"Delivery failed at {DateTime.UtcNow:u}. Retries: {item.RetryCount}/{item.MaxRetries}";
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

