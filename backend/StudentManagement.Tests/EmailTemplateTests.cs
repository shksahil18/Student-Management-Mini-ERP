using Microsoft.Extensions.Options;
using StudentManagement.Api.DTOs.Fees;
using StudentManagement.Api.DTOs.Students;
using StudentManagement.Api.Entities;
using StudentManagement.Api.Services.Implementations;
using StudentManagement.Api.Services.Interfaces;
using Xunit;

namespace StudentManagement.Tests;

public class EmailTemplateTests
{
    private readonly EmailTemplateService _templateService;

    public EmailTemplateTests()
    {
        var options = Options.Create(new EmailOptions
        {
            InstitutionName = "Known Institute of Technology",
            FrontendBaseUrl = "http://localhost:5173",
            SenderEmail = "admin@institution.edu",
            SupportContact = "support@institution.edu"
        });
        _templateService = new EmailTemplateService(options);
    }

    [Fact]
    public void BuildAdmissionConfirmationEmail_EncodesHtml_AndIncludesKeyData()
    {
        var studentDto = new StudentDto
        {
            StudentCode = "STD-2026-999",
            FirstName = "<Jane & John>",
            LastName = "Doe",
            Email = "jane@example.com",
            Course = "Computer Science & Engineering",
            AdmissionDate = new DateTime(2026, 9, 1),
            IsActive = true
        };

        var email = _templateService.BuildAdmissionConfirmationEmail(studentDto);

        Assert.Equal("jane@example.com", email.ToEmail);
        Assert.Contains("Admission Confirmation", email.Subject);
        Assert.Contains("STD-2026-999", email.Subject);
        Assert.Contains("&lt;Jane &amp; John&gt;", email.HtmlBody);
        Assert.DoesNotContain("<Jane & John>", email.HtmlBody);
        Assert.Contains("STD-2026-999", email.HtmlBody);
        Assert.Contains("Computer Science &amp; Engineering", email.HtmlBody);
        Assert.Contains("http://localhost:5173/login", email.HtmlBody);
    }

    [Fact]
    public void BuildFeeReceiptEmail_ContainsReceiptDetails_AndHtmlEncodes()
    {
        var receipt = new ReceiptDto
        {
            ReceiptNumber = "REC-20261017-0042",
            StudentId = 1,
            StudentCode = "STD-2026-001",
            StudentName = "Robert Green",
            Course = "Mechanical Engineering",
            Amount = 15000m,
            PaymentDate = new DateTime(2026, 10, 17),
            PaymentMethod = "Bank Transfer",
            FeeId = 1,
            FeeTotalAmount = 50000m,
            FeePaidAmount = 15000m,
            RemainingOutstanding = 35000m,
            FeeStatus = "Partial",
            Notes = "First Installment <safe>"
        };

        var email = _templateService.BuildFeeReceiptEmail(receipt, "robert@example.com");

        Assert.Equal("robert@example.com", email.ToEmail);
        Assert.Contains("REC-20261017-0042", email.Subject);
        Assert.Contains("15,000.00", email.HtmlBody);
        Assert.Contains("35,000.00", email.HtmlBody);
        Assert.Contains("First Installment &lt;safe&gt;", email.HtmlBody);
        Assert.Equal("FeeReceipt", email.EmailType);
        Assert.Equal("REC-20261017-0042", email.ReferenceId);
    }

    [Fact]
    public void BuildUpcomingFeeReminderEmail_ContainsUpcomingDetails()
    {
        var student = new Student
        {
            Id = 10,
            StudentCode = "STD-010",
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@example.com",
            Course = "Information Technology",
            IsActive = true
        };

        var fee = new Fee
        {
            Id = 5,
            StudentId = 10,
            TotalAmount = 40000m,
            PaidAmount = 10000m,
            OutstandingAmount = 30000m,
            DueDate = DateTime.UtcNow.AddDays(2),
            Status = "Partial"
        };

        var email = _templateService.BuildUpcomingFeeReminderEmail(student, fee, 2);

        Assert.Equal("alice@example.com", email.ToEmail);
        Assert.Contains("Due in 2 Days", email.Subject);
        Assert.Contains("30,000.00", email.HtmlBody);
        Assert.Contains("2 days", email.HtmlBody);
        Assert.Equal("FeeReminder", email.EmailType);
    }

    [Fact]
    public void BuildOverdueFeeReminderEmail_ContainsOverdueNotice()
    {
        var student = new Student
        {
            Id = 11,
            StudentCode = "STD-011",
            FirstName = "Bob",
            LastName = "Brown",
            Email = "bob@example.com",
            Course = "Civil Engineering",
            IsActive = true
        };

        var fee = new Fee
        {
            Id = 6,
            StudentId = 11,
            TotalAmount = 40000m,
            PaidAmount = 0m,
            OutstandingAmount = 40000m,
            DueDate = DateTime.UtcNow.AddDays(-5),
            Status = "Pending"
        };

        var email = _templateService.BuildOverdueFeeReminderEmail(student, fee, 5);

        Assert.Equal("bob@example.com", email.ToEmail);
        Assert.Contains("OVERDUE NOTICE", email.Subject);
        Assert.Contains("40,000.00", email.HtmlBody);
        Assert.Contains("5 days overdue", email.HtmlBody);
        Assert.Equal("FeeReminder", email.EmailType);
    }

    [Fact]
    public void BuildEmailVerificationEmail_IncludesVerificationLink()
    {
        var link = "http://localhost:5173/verify-email?token=abcdef123456";
        var email = _templateService.BuildEmailVerificationEmail("staff@example.com", "staff_user", link);

        Assert.Equal("staff@example.com", email.ToEmail);
        Assert.Contains("Verify Your Institutional Account", email.Subject);
        Assert.Contains(link, email.HtmlBody);
    }

    [Fact]
    public void BuildPasswordResetEmail_IncludesResetLink()
    {
        var link = "http://localhost:5173/reset-password?token=resetxyz123";
        var email = _templateService.BuildPasswordResetEmail("admin@example.com", "admin", link);

        Assert.Equal("admin@example.com", email.ToEmail);
        Assert.Contains("Password Reset Request", email.Subject);
        Assert.Contains(link, email.HtmlBody);
    }
}
