using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using StudentManagement.Api.DTOs.Fees;
using StudentManagement.Api.DTOs.Students;
using StudentManagement.Api.Entities;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Services.Implementations;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly EmailOptions _options;

    public EmailTemplateService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    private string WrapHtmlLayout(string title, string headerBadge, string contentHtml)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{WebUtility.HtmlEncode(title)}</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px; color: #1e293b; line-height: 1.6; }}
        .card {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 12px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); }}
        .header {{ background: linear-gradient(135deg, #1b8e63 0%, #29ca8e 100%); color: #ffffff; padding: 28px 24px; text-align: left; }}
        .badge {{ display: inline-block; background: rgba(255,255,255,0.2); padding: 4px 10px; border-radius: 16px; font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 8px; }}
        .title {{ font-size: 20px; font-weight: 800; margin: 0; }}
        .body {{ padding: 28px 24px; }}
        .info-table {{ width: 100%; border-collapse: collapse; margin: 20px 0; background: #f8fafc; border-radius: 8px; overflow: hidden; }}
        .info-table td {{ padding: 12px 16px; border-bottom: 1px solid #e2e8f0; font-size: 14px; }}
        .info-table tr:last-child td {{ border-bottom: none; }}
        .label {{ font-weight: 600; color: #64748b; width: 40%; }}
        .value {{ font-weight: 700; color: #0f172a; text-align: right; }}
        .btn {{ display: inline-block; background-color: #29ca8e; color: #ffffff !important; font-weight: 700; text-decoration: none; padding: 12px 24px; border-radius: 6px; font-size: 14px; margin-top: 16px; }}
        .footer {{ padding: 20px 24px; background-color: #f1f5f9; border-top: 1px solid #e2e8f0; font-size: 12px; color: #64748b; text-align: center; }}
        .highlight {{ color: #dc2626; font-weight: 800; }}
        .success {{ color: #16a34a; font-weight: 800; }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""header"">
            <span class=""badge"">{WebUtility.HtmlEncode(headerBadge)}</span>
            <h1 class=""title"">{WebUtility.HtmlEncode(title)}</h1>
        </div>
        <div class=""body"">
            {contentHtml}
        </div>
        <div class=""footer"">
            <p><strong>{WebUtility.HtmlEncode(_options.InstitutionName)}</strong></p>
            <p>Need assistance? Contact support at <a href=""mailto:{WebUtility.HtmlEncode(_options.SupportContact)}"">{WebUtility.HtmlEncode(_options.SupportContact)}</a></p>
            <p>&copy; {DateTime.UtcNow.Year} Student Management Mini-ERP. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public EmailMessageDto BuildAdmissionConfirmationEmail(StudentDto student)
    {
        var fullName = $"{student.FirstName} {student.LastName}".Trim();
        var subject = $"Admission Confirmation — {student.StudentCode} | {_options.InstitutionName}";

        var html = $@"
            <p>Dear <strong>{WebUtility.HtmlEncode(fullName)}</strong>,</p>
            <p>Congratulations! We are delighted to inform you that your admission to <strong>{WebUtility.HtmlEncode(_options.InstitutionName)}</strong> has been successfully processed.</p>
            
            <table class=""info-table"">
                <tr><td class=""label"">Student ID Code</td><td class=""value"">{WebUtility.HtmlEncode(student.StudentCode)}</td></tr>
                <tr><td class=""label"">Enrolled Program</td><td class=""value"">{WebUtility.HtmlEncode(student.Course)}</td></tr>
                <tr><td class=""label"">Admission Date</td><td class=""value"">{student.AdmissionDate:dd MMM yyyy}</td></tr>
                <tr><td class=""label"">Enrollment Status</td><td class=""value success"">Confirmed / Active</td></tr>
            </table>

            <p><strong>Next Steps:</strong></p>
            <ul>
                <li>Keep your Student Code handy for all fee payments, library access, and course registrations.</li>
                <li>Visit the institutional portal to review your curriculum schedule and fee ledger.</li>
            </ul>

            <div style=""text-align: center; margin-top: 24px;"">
                <a href=""{_options.FrontendBaseUrl}/login"" class=""btn"">Access Student Portal</a>
            </div>";

        var plainText = $@"Dear {fullName},

Congratulations! Your admission to {_options.InstitutionName} has been successfully processed.

Student Code: {student.StudentCode}
Enrolled Course: {student.Course}
Admission Date: {student.AdmissionDate:yyyy-MM-dd}
Status: Active

Please retain your Student Code ({student.StudentCode}) for all future institutional transactions.
Access your portal: {_options.FrontendBaseUrl}/login

Support Contact: {_options.SupportContact}
{_options.InstitutionName}";

        return new EmailMessageDto
        {
            ToEmail = student.Email,
            ToName = fullName,
            Subject = subject,
            HtmlBody = WrapHtmlLayout("Admission Confirmation", "Official Notice", html),
            PlainTextBody = plainText,
            EmailType = "AdmissionConfirmation",
            ReferenceId = student.StudentCode
        };
    }

    public EmailMessageDto BuildFeeReceiptEmail(ReceiptDto receipt, string studentEmail)
    {
        var subject = $"Official Payment Receipt [{receipt.ReceiptNumber}] — ₹{receipt.Amount:N2}";

        var html = $@"
            <p>Dear <strong>{WebUtility.HtmlEncode(receipt.StudentName)}</strong>,</p>
            <p>Thank you for your payment. We have successfully recorded your fee installment. Here is your official e-receipt:</p>
            
            <table class=""info-table"">
                <tr><td class=""label"">Receipt Number</td><td class=""value"">{WebUtility.HtmlEncode(receipt.ReceiptNumber)}</td></tr>
                <tr><td class=""label"">Payment Date</td><td class=""value"">{receipt.PaymentDate:dd MMM yyyy, hh:mm tt} UTC</td></tr>
                <tr><td class=""label"">Student Code</td><td class=""value"">{WebUtility.HtmlEncode(receipt.StudentCode)}</td></tr>
                <tr><td class=""label"">Program / Course</td><td class=""value"">{WebUtility.HtmlEncode(receipt.Course)}</td></tr>
                <tr><td class=""label"">Payment Method</td><td class=""value"">{WebUtility.HtmlEncode(receipt.PaymentMethod)}</td></tr>
                <tr><td class=""label"">Amount Paid</td><td class=""value success"">₹{receipt.Amount:N2}</td></tr>
                <tr><td class=""label"">Total Course Fee</td><td class=""value"">₹{receipt.FeeTotalAmount:N2}</td></tr>
                <tr><td class=""label"">Total Paid to Date</td><td class=""value"">₹{receipt.FeePaidAmount:N2}</td></tr>
                <tr><td class=""label"">Remaining Outstanding</td><td class=""value {(receipt.RemainingOutstanding > 0 ? "highlight" : "success")}"">₹{receipt.RemainingOutstanding:N2}</td></tr>
                <tr><td class=""label"">Fee Status</td><td class=""value"">{WebUtility.HtmlEncode(receipt.FeeStatus)}</td></tr>
            </table>

            {(string.IsNullOrWhiteSpace(receipt.Notes) ? "" : $"<p><em>Transaction Notes: {WebUtility.HtmlEncode(receipt.Notes)}</em></p>")}

            <div style=""text-align: center; margin-top: 24px;"">
                <a href=""{_options.FrontendBaseUrl}/fees"" class=""btn"">View Fee Statement</a>
            </div>";

        var plainText = $@"Official Fee Payment Receipt
Receipt No: {receipt.ReceiptNumber}
Date: {receipt.PaymentDate:yyyy-MM-dd HH:mm} UTC
Student: {receipt.StudentName} ({receipt.StudentCode})
Course: {receipt.Course}
Amount Paid: INR {receipt.Amount:N2} via {receipt.PaymentMethod}
Remaining Balance: INR {receipt.RemainingOutstanding:N2}
Status: {receipt.FeeStatus}

View online: {_options.FrontendBaseUrl}/fees
{_options.InstitutionName} - {_options.SupportContact}";

        return new EmailMessageDto
        {
            ToEmail = studentEmail,
            ToName = receipt.StudentName,
            Subject = subject,
            HtmlBody = WrapHtmlLayout("Payment Receipt", "Transaction Confirmed", html),
            PlainTextBody = plainText,
            EmailType = "FeeReceipt",
            ReferenceId = receipt.ReceiptNumber
        };
    }

    public EmailMessageDto BuildUpcomingFeeReminderEmail(Student student, Fee fee, int daysLeft)
    {
        var fullName = $"{student.FirstName} {student.LastName}".Trim();
        var subject = $"Reminder: Tuition Fee Payment Due in {daysLeft} Days — {student.StudentCode}";

        var html = $@"
            <p>Dear <strong>{WebUtility.HtmlEncode(fullName)}</strong>,</p>
            <p>This is a gentle reminder that an upcoming installment for your enrolled course (<strong>{WebUtility.HtmlEncode(student.Course)}</strong>) is due in <strong>{daysLeft} days</strong>.</p>
            
            <table class=""info-table"">
                <tr><td class=""label"">Student Code</td><td class=""value"">{WebUtility.HtmlEncode(student.StudentCode)}</td></tr>
                <tr><td class=""label"">Due Date</td><td class=""value highlight"">{fee.DueDate:dd MMM yyyy}</td></tr>
                <tr><td class=""label"">Total Assigned Fee</td><td class=""value"">₹{fee.TotalAmount:N2}</td></tr>
                <tr><td class=""label"">Paid so Far</td><td class=""value"">₹{fee.PaidAmount:N2}</td></tr>
                <tr><td class=""label"">Outstanding Balance</td><td class=""value highlight"">₹{fee.OutstandingAmount:N2}</td></tr>
            </table>

            <p>To avoid late penalties or administrative holds, please make your payment before the due date.</p>

            <div style=""text-align: center; margin-top: 24px;"">
                <a href=""{_options.FrontendBaseUrl}/fees"" class=""btn"">Pay Fee Online</a>
            </div>";

        var plainText = $@"Upcoming Fee Reminder
Dear {fullName},
Your tuition payment of INR {fee.OutstandingAmount:N2} for {student.Course} is due on {fee.DueDate:yyyy-MM-dd} (in {daysLeft} days).
Please clear your balance to prevent late penalties.
Pay online: {_options.FrontendBaseUrl}/fees
Support: {_options.SupportContact}";

        return new EmailMessageDto
        {
            ToEmail = student.Email,
            ToName = fullName,
            Subject = subject,
            HtmlBody = WrapHtmlLayout("Upcoming Fee Reminder", "Payment Notice", html),
            PlainTextBody = plainText,
            EmailType = "FeeReminder",
            ReferenceId = $"UPCOMING-FEE-{fee.Id}-{DateTime.UtcNow:yyyyMMdd}"
        };
    }

    public EmailMessageDto BuildOverdueFeeReminderEmail(Student student, Fee fee, int daysOverdue)
    {
        var fullName = $"{student.FirstName} {student.LastName}".Trim();
        var subject = $"OVERDUE NOTICE: Tuition Fee Past Due by {daysOverdue} Days — {student.StudentCode}";

        var html = $@"
            <p>Dear <strong>{WebUtility.HtmlEncode(fullName)}</strong>,</p>
            <p>Our records show that your tuition fee installment was due on <strong>{fee.DueDate:dd MMM yyyy}</strong> and is currently <strong>{daysOverdue} days overdue</strong>.</p>
            
            <table class=""info-table"">
                <tr><td class=""label"">Student Code</td><td class=""value"">{WebUtility.HtmlEncode(student.StudentCode)}</td></tr>
                <tr><td class=""label"">Original Due Date</td><td class=""value"">{fee.DueDate:dd MMM yyyy}</td></tr>
                <tr><td class=""label"">Days Overdue</td><td class=""value highlight"">{daysOverdue} days</td></tr>
                <tr><td class=""label"">Remaining Balance</td><td class=""value highlight"">₹{fee.OutstandingAmount:N2}</td></tr>
            </table>

            <p>Please settle the outstanding balance immediately to avoid examination restrictions or academic service interruptions. If you have already made this payment, kindly contact the accounts desk with your transaction receipt.</p>

            <div style=""text-align: center; margin-top: 24px;"">
                <a href=""{_options.FrontendBaseUrl}/fees"" class=""btn"">Pay Outstanding Balance</a>
            </div>";

        var plainText = $@"OVERDUE FEE NOTICE
Dear {fullName},
Your fee payment of INR {fee.OutstandingAmount:N2} was due on {fee.DueDate:yyyy-MM-dd} and is {daysOverdue} days overdue.
Please settle your balance immediately.
Pay online: {_options.FrontendBaseUrl}/fees
Support: {_options.SupportContact}";

        return new EmailMessageDto
        {
            ToEmail = student.Email,
            ToName = fullName,
            Subject = subject,
            HtmlBody = WrapHtmlLayout("Overdue Fee Notice", "Urgent Action Required", html),
            PlainTextBody = plainText,
            EmailType = "FeeReminder",
            ReferenceId = $"OVERDUE-FEE-{fee.Id}-{DateTime.UtcNow:yyyyMMdd}"
        };
    }

    public EmailMessageDto BuildEmailVerificationEmail(string toEmail, string username, string verificationLink)
    {
        var subject = $"Verify Your Institutional Account — {_options.InstitutionName}";

        var html = $@"
            <p>Hello <strong>{WebUtility.HtmlEncode(username)}</strong>,</p>
            <p>Thank you for registering an account on the <strong>{WebUtility.HtmlEncode(_options.InstitutionName)}</strong> ERP system.</p>
            <p>Please verify your email address to ensure your account security and enable administrative access.</p>

            <div style=""text-align: center; margin: 32px 0;"">
                <a href=""{verificationLink}"" class=""btn"">Verify My Email Address</a>
            </div>

            <p style=""font-size: 13px; color: #64748b;"">This verification link will expire in <strong>{_options.VerificationTokenExpiryHours} hours</strong>. If you did not create this account, please ignore this email.</p>
            <p style=""font-size: 12px; color: #94a3b8; word-break: break-all;"">Direct link: {verificationLink}</p>";

        var plainText = $@"Hello {username},
Please verify your email address for {_options.InstitutionName} Student ERP:
{verificationLink}
This link expires in {_options.VerificationTokenExpiryHours} hours.
If you did not request this, please disregard this email.";

        return new EmailMessageDto
        {
            ToEmail = toEmail,
            ToName = username,
            Subject = subject,
            HtmlBody = WrapHtmlLayout("Email Verification", "Account Security", html),
            PlainTextBody = plainText,
            EmailType = "EmailVerification",
            ReferenceId = username
        };
    }

    public EmailMessageDto BuildPasswordResetEmail(string toEmail, string username, string resetLink)
    {
        var subject = $"Password Reset Request — {_options.InstitutionName}";

        var html = $@"
            <p>Hello <strong>{WebUtility.HtmlEncode(username)}</strong>,</p>
            <p>We received a request to reset the password for your account on <strong>{WebUtility.HtmlEncode(_options.InstitutionName)}</strong>.</p>
            <p>Click the button below to securely set a new password:</p>

            <div style=""text-align: center; margin: 32px 0;"">
                <a href=""{resetLink}"" class=""btn"">Reset Password</a>
            </div>

            <p style=""font-size: 13px; color: #64748b;"">This link is single-use and will expire in <strong>{_options.PasswordResetTokenExpiryMinutes} minutes</strong>. If you did not request a password reset, no action is needed; your current password remains secure.</p>
            <p style=""font-size: 12px; color: #94a3b8; word-break: break-all;"">Direct link: {resetLink}</p>";

        var plainText = $@"Hello {username},
We received a request to reset your password for {_options.InstitutionName} ERP.
Reset your password here:
{resetLink}
This single-use link expires in {_options.PasswordResetTokenExpiryMinutes} minutes.
If you did not request this, no action is needed.";

        return new EmailMessageDto
        {
            ToEmail = toEmail,
            ToName = username,
            Subject = subject,
            HtmlBody = WrapHtmlLayout("Password Reset Request", "Security Verification", html),
            PlainTextBody = plainText,
            EmailType = "PasswordReset",
            ReferenceId = username
        };
    }
}

