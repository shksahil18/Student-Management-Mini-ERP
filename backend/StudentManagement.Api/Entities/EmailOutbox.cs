namespace StudentManagement.Api.Entities;

public enum EmailOutboxStatus
{
    Pending = 0,
    Processing = 1,
    Sent = 2,
    Failed = 3
}

public class EmailOutbox
{
    public int Id { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string PlainTextBody { get; set; } = string.Empty;
    public string EmailType { get; set; } = string.Empty; // "AdmissionConfirmation", "FeeReceipt", "FeeReminder", "EmailVerification", "PasswordReset"
    public string? ReferenceId { get; set; } // e.g., StudentCode or ReceiptNumber (for idempotency)
    public EmailOutboxStatus Status { get; set; } = EmailOutboxStatus.Pending;
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}

