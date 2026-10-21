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

public class SmtpEmailServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task QueueEmailAsync_InsertsPendingRecordInOutbox()
    {
        using var db = CreateInMemoryDbContext();
        var mockLogger = new Mock<ILogger<SmtpEmailService>>();
        var emailOptions = Options.Create(new EmailOptions
        {
            SmtpHost = "localhost",
            SmtpUsername = "" // unconfigured fallback
        });

        var service = new SmtpEmailService(db, emailOptions, mockLogger.Object);

        var msg = new EmailMessageDto
        {
            ToEmail = "student@example.com",
            ToName = "John Doe",
            Subject = "Admission Confirmation",
            HtmlBody = "<p>Welcome to ERP</p>",
            PlainTextBody = "Welcome to ERP",
            EmailType = "AdmissionConfirmation",
            ReferenceId = "STD-2026-101"
        };

        await service.QueueEmailAsync(msg);

        var queued = await db.EmailOutbox.FirstOrDefaultAsync(e => e.ReferenceId == "STD-2026-101");
        Assert.NotNull(queued);
        Assert.Equal("student@example.com", queued.ToEmail);
        Assert.Equal("Admission Confirmation", queued.Subject);
        Assert.Equal(EmailOutboxStatus.Pending, queued.Status);
        Assert.Equal(0, queued.RetryCount);
    }

    [Fact]
    public async Task QueueEmailAsync_DuplicateReferenceId_IsSuppressed()
    {
        using var db = CreateInMemoryDbContext();
        var mockLogger = new Mock<ILogger<SmtpEmailService>>();
        var emailOptions = Options.Create(new EmailOptions());

        var service = new SmtpEmailService(db, emailOptions, mockLogger.Object);

        var msg1 = new EmailMessageDto
        {
            ToEmail = "a@example.com",
            Subject = "Receipt #1",
            EmailType = "FeeReceipt",
            ReferenceId = "REC-999"
        };

        var msg2 = new EmailMessageDto
        {
            ToEmail = "a@example.com",
            Subject = "Receipt #2",
            EmailType = "FeeReceipt",
            ReferenceId = "REC-999"
        };

        await service.QueueEmailAsync(msg1);
        await service.QueueEmailAsync(msg2);

        var count = await db.EmailOutbox.CountAsync(e => e.ReferenceId == "REC-999");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ProcessPendingOutboxAsync_WhenSmtpUnconfigured_SimulatesSafeLogAndMarksSent()
    {
        using var db = CreateInMemoryDbContext();
        var mockLogger = new Mock<ILogger<SmtpEmailService>>();
        var emailOptions = Options.Create(new EmailOptions
        {
            SmtpHost = "localhost",
            SmtpUsername = "" // Unconfigured triggers simulation warning and returns true
        });

        db.EmailOutbox.Add(new EmailOutbox
        {
            ToEmail = "test@example.com",
            Subject = "Test Email",
            HtmlBody = "<p>Test</p>",
            EmailType = "TestType",
            Status = EmailOutboxStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new SmtpEmailService(db, emailOptions, mockLogger.Object);
        await service.ProcessPendingOutboxAsync(10);

        var reloaded = await db.EmailOutbox.FirstAsync();
        Assert.Equal(EmailOutboxStatus.Sent, reloaded.Status);
        Assert.NotNull(reloaded.ProcessedAt);
        Assert.Null(reloaded.ErrorMessage);
    }
}

