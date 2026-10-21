using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StudentManagement.Api.Data;
using StudentManagement.Api.Entities;
using StudentManagement.Api.Services.Implementations;
using StudentManagement.Api.Services.Interfaces;
using Xunit;

namespace StudentManagement.Tests;

public class FeeReminderServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ProcessUpcomingAndOverdueRemindersAsync_UpcomingFeeWithinLeadTime_QueuesReminder()
    {
        using var db = CreateInMemoryDbContext();

        var student = new Student
        {
            FirstName = "Alice",
            LastName = "Johnson",
            StudentCode = "ENR-001",
            Email = "alice@example.com",
            Course = "Computer Science",
            IsActive = true
        };
        db.Students.Add(student);
        await db.SaveChangesAsync();

        var fee = new Fee
        {
            StudentId = student.Id,
            TotalAmount = 10000m,
            PaidAmount = 2000m,
            OutstandingAmount = 8000m,
            DueDate = DateTime.UtcNow.AddDays(2), // 2 days away <= 3 days lead time
            Status = "Partial",
            CreatedAt = DateTime.UtcNow
        };
        db.Fees.Add(fee);
        await db.SaveChangesAsync();

        var mockEmailService = new Mock<IEmailService>();
        var mockTemplateService = new Mock<IEmailTemplateService>();
        var mockLogger = new Mock<ILogger<FeeReminderService>>();

        mockTemplateService.Setup(t => t.BuildUpcomingFeeReminderEmail(
            It.IsAny<Student>(), It.IsAny<Fee>(), It.IsAny<int>()))
            .Returns(new EmailMessageDto
            {
                ToEmail = "alice@example.com",
                Subject = "Upcoming Fee Reminder",
                EmailType = "FeeReminder"
            });

        var options = Options.Create(new EmailOptions
        {
            FrontendBaseUrl = "http://localhost:5173",
            UpcomingReminderLeadDays = 3,
            OverdueReminderCooldownDays = 7
        });

        var reminderService = new FeeReminderService(db, mockEmailService.Object, mockTemplateService.Object, options, mockLogger.Object);

        var result = await reminderService.ProcessUpcomingAndOverdueRemindersAsync();

        Assert.Equal(1, result);

        mockEmailService.Verify(e => e.QueueEmailAsync(
            It.Is<EmailMessageDto>(m => m.ToEmail == "alice@example.com" && m.EmailType == "FeeReminder"),
            It.IsAny<CancellationToken>()), Times.Once);

        // Verify FeeReminderLog record was created
        var log = await db.FeeReminderLogs.FirstOrDefaultAsync(l => l.FeeId == fee.Id);
        Assert.NotNull(log);
        Assert.Equal(ReminderType.Upcoming, log.ReminderType);
        Assert.Equal(8000m, log.OutstandingAmount);
    }

    [Fact]
    public async Task ProcessUpcomingAndOverdueRemindersAsync_FullyPaidOrInactiveStudent_Suppressed()
    {
        using var db = CreateInMemoryDbContext();

        var inactiveStudent = new Student
        {
            FirstName = "Inactive",
            LastName = "Student",
            StudentCode = "ENR-002",
            Email = "inactive@example.com",
            Course = "Arts",
            IsActive = false // Inactive
        };
        db.Students.Add(inactiveStudent);

        var activeStudent = new Student
        {
            FirstName = "Fully",
            LastName = "Paid",
            StudentCode = "ENR-003",
            Email = "paid@example.com",
            Course = "Business",
            IsActive = true
        };
        db.Students.Add(activeStudent);
        await db.SaveChangesAsync();

        // Fee for inactive student
        db.Fees.Add(new Fee
        {
            StudentId = inactiveStudent.Id,
            TotalAmount = 5000m,
            PaidAmount = 0m,
            OutstandingAmount = 5000m,
            DueDate = DateTime.UtcNow.AddDays(-2), // overdue
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        });

        // Fully paid fee for active student
        db.Fees.Add(new Fee
        {
            StudentId = activeStudent.Id,
            TotalAmount = 5000m,
            PaidAmount = 5000m,
            OutstandingAmount = 0m, // fully paid
            DueDate = DateTime.UtcNow.AddDays(-2),
            Status = "Paid",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var mockEmailService = new Mock<IEmailService>();
        var mockTemplateService = new Mock<IEmailTemplateService>();
        var mockLogger = new Mock<ILogger<FeeReminderService>>();

        var options = Options.Create(new EmailOptions());
        var reminderService = new FeeReminderService(db, mockEmailService.Object, mockTemplateService.Object, options, mockLogger.Object);

        var result = await reminderService.ProcessUpcomingAndOverdueRemindersAsync();

        Assert.Equal(0, result);
        mockEmailService.Verify(e => e.QueueEmailAsync(
            It.IsAny<EmailMessageDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessUpcomingAndOverdueRemindersAsync_OverdueWithinCooldown_DoesNotDuplicate()
    {
        using var db = CreateInMemoryDbContext();

        var student = new Student
        {
            FirstName = "Bob",
            LastName = "Miller",
            StudentCode = "ENR-004",
            Email = "bob@example.com",
            Course = "Engineering",
            IsActive = true
        };
        db.Students.Add(student);
        await db.SaveChangesAsync();

        var fee = new Fee
        {
            StudentId = student.Id,
            TotalAmount = 20000m,
            PaidAmount = 5000m,
            OutstandingAmount = 15000m,
            DueDate = DateTime.UtcNow.AddDays(-10), // overdue 10 days
            Status = "Partial",
            CreatedAt = DateTime.UtcNow
        };
        db.Fees.Add(fee);
        await db.SaveChangesAsync();

        // Existing reminder sent only 2 days ago (less than 7-day cooldown)
        db.FeeReminderLogs.Add(new FeeReminderLog
        {
            FeeId = fee.Id,
            StudentId = student.Id,
            ReminderType = ReminderType.Overdue,
            OutstandingAmount = 15000m,
            SentAt = DateTime.UtcNow.AddDays(-2)
        });
        await db.SaveChangesAsync();

        var mockEmailService = new Mock<IEmailService>();
        var mockTemplateService = new Mock<IEmailTemplateService>();
        var mockLogger = new Mock<ILogger<FeeReminderService>>();

        var options = Options.Create(new EmailOptions { OverdueReminderCooldownDays = 7 });
        var reminderService = new FeeReminderService(db, mockEmailService.Object, mockTemplateService.Object, options, mockLogger.Object);

        var result = await reminderService.ProcessUpcomingAndOverdueRemindersAsync();

        Assert.Equal(0, result);
        mockEmailService.Verify(e => e.QueueEmailAsync(
            It.IsAny<EmailMessageDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

