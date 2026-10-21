namespace StudentManagement.Api.Services.Interfaces;

public class EmailMessageDto
{
    public string ToEmail { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string PlainTextBody { get; set; } = string.Empty;
    public string EmailType { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
}

public interface IEmailService
{
    Task<bool> SendEmailDirectAsync(EmailMessageDto message, CancellationToken cancellationToken = default);
    Task QueueEmailAsync(EmailMessageDto message, CancellationToken cancellationToken = default);
    Task ProcessPendingOutboxAsync(int batchSize = 10, CancellationToken cancellationToken = default);
}

