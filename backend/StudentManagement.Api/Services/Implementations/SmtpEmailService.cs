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

    private static string ExtractSafeDomain(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "unknown";
        var atIndex = email.LastIndexOf('@');
        return atIndex >= 0 && atIndex < email.Length - 1
            ? email[(atIndex + 1)..].ToLowerInvariant()
            : "invalid";
    }

    private static bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email.Trim());
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }

    private SecureSocketOptions GetSecureSocketOptions()
    {
        // Port 465 is legacy implicit SSL/TLS
        if (_options.SmtpPort == 465)
        {
            return SecureSocketOptions.SslOnConnect;
        }

        // Port 587 is standard submission with STARTTLS
        if (_options.SmtpPort == 587)
        {
            return SecureSocketOptions.StartTls;
        }

        if (_options.EnableSsl)
        {
            return SecureSocketOptions.Auto;
        }

        return _options.SmtpPort == 25 ? SecureSocketOptions.None : SecureSocketOptions.Auto;
    }

    public async Task<bool> SendEmailDirectAsync(EmailMessageDto message, CancellationToken cancellationToken = default)
    {
        var domain = ExtractSafeDomain(message.ToEmail);

        if (!IsValidEmailFormat(message.ToEmail))
        {
            _logger.LogWarning("Invalid email format rejected: recipient domain '{Domain}', type [{EmailType}]", domain, message.EmailType);
            return false;
        }

        // Validate that SMTP configuration is provided. Never simulate successful delivery!
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) ||
            string.IsNullOrWhiteSpace(_options.SenderEmail) ||
            (_options.SmtpHost.Equals("localhost", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(_options.SmtpUsername)))
        {
            _logger.LogError("SMTP delivery failed: SMTP is unconfigured or set to localhost without credentials. Cannot deliver [{EmailType}] to @{Domain}",
                message.EmailType, domain);
            return false;
        }

        try
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail.Trim()));
            mimeMessage.To.Add(new MailboxAddress(message.ToName, message.ToEmail.Trim()));
            mimeMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message.HtmlBody,
                TextBody = string.IsNullOrWhiteSpace(message.PlainTextBody) ? message.Subject : message.PlainTextBody
            };

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 15000; // 15-second timeout safeguard

            var socketOptions = GetSecureSocketOptions();
            _logger.LogInformation("Connecting to SMTP [{Host}:{Port}] with security {Security} for domain @{Domain}...",
                _options.SmtpHost, _options.SmtpPort, socketOptions, domain);

            await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, socketOptions, cancellationToken);
            _logger.LogInformation("SMTP connection established with {Host}:{Port}", _options.SmtpHost, _options.SmtpPort);

            if (!string.IsNullOrWhiteSpace(_options.SmtpUsername) && !string.IsNullOrWhiteSpace(_options.SmtpPassword))
            {
                await client.AuthenticateAsync(_options.SmtpUsername.Trim(), _options.SmtpPassword.Trim(), cancellationToken);
                _logger.LogInformation("SMTP authentication successful for {Username}", _options.SmtpUsername);
            }

            await client.SendAsync(mimeMessage, cancellationToken);
            _logger.LogInformation("SMTP submission successful: [{EmailType}] accepted by server for @{Domain}", message.EmailType, domain);

            await client.DisconnectAsync(true, cancellationToken);
            return true;
        }
        catch (SmtpCommandException ex)
        {
            _logger.LogError(ex, "SMTP Command Rejected ({StatusCode}) for @{Domain} [{EmailType}]: {Message}",
                ex.StatusCode, domain, message.EmailType, ex.Message);
            return false;
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError(ex, "SMTP Authentication Failed on {Host} for [{EmailType}]: {Message}",
                _options.SmtpHost, message.EmailType, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email [{EmailType}] to @{Domain}: {Message}",
                message.EmailType, domain, ex.Message);
            return false;
        }
    }

    public async Task QueueEmailAsync(EmailMessageDto message, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(message.ReferenceId))
        {
            var exists = await _context.EmailOutbox
                .AnyAsync(o => o.EmailType == message.EmailType &&
                               o.ReferenceId == message.ReferenceId &&
                               (o.Status == EmailOutboxStatus.Sent || o.Status == EmailOutboxStatus.Pending || o.Status == EmailOutboxStatus.Processing),
                          cancellationToken);

            if (exists)
            {
                _logger.LogInformation("Email [{EmailType}] with ReferenceId '{ReferenceId}' already exists in Outbox. Suppressing duplicate queuing.",
                    message.EmailType, message.ReferenceId);
                return;
            }
        }

        var outboxItem = new EmailOutbox
        {
            ToEmail = message.ToEmail.Trim(),
            ToName = message.ToName ?? string.Empty,
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

        _logger.LogInformation("Queued email [{EmailType}] for @{Domain} (Outbox ID: {Id})",
            outboxItem.EmailType, ExtractSafeDomain(outboxItem.ToEmail), outboxItem.Id);
    }

    public async Task<bool> QueueAndSendAsync(EmailMessageDto message, CancellationToken cancellationToken = default)
    {
        var domain = ExtractSafeDomain(message.ToEmail);

        if (!string.IsNullOrWhiteSpace(message.ReferenceId))
        {
            var exists = await _context.EmailOutbox
                .AnyAsync(o => o.EmailType == message.EmailType &&
                               o.ReferenceId == message.ReferenceId &&
                               (o.Status == EmailOutboxStatus.Sent || o.Status == EmailOutboxStatus.Pending || o.Status == EmailOutboxStatus.Processing),
                          cancellationToken);

            if (exists)
            {
                _logger.LogInformation("Email [{EmailType}] with ReferenceId '{ReferenceId}' already recorded in Outbox.",
                    message.EmailType, message.ReferenceId);
                return true;
            }
        }

        var outboxItem = new EmailOutbox
        {
            ToEmail = message.ToEmail.Trim(),
            ToName = message.ToName ?? string.Empty,
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

        _logger.LogInformation("Persisted Outbox item #{Id} for [{EmailType}] to @{Domain}. Dispatching immediately...",
            outboxItem.Id, outboxItem.EmailType, domain);

        var directResult = await SendEmailDirectAsync(message, cancellationToken);
        if (directResult)
        {
            outboxItem.Status = EmailOutboxStatus.Sent;
            outboxItem.ProcessedAt = DateTime.UtcNow;
            outboxItem.ErrorMessage = null;
        }
        else
        {
            outboxItem.RetryCount = 1;
            outboxItem.Status = EmailOutboxStatus.Pending; // Keep pending so background service will retry
            outboxItem.ErrorMessage = $"Direct delivery attempt failed at {DateTime.UtcNow:u}. Retries: 1/{outboxItem.MaxRetries}. Queued for background retry.";
        }

        await _context.SaveChangesAsync(cancellationToken);
        return directResult;
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


