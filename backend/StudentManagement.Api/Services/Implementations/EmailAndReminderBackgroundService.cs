using Microsoft.Extensions.Options;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Services.Implementations;

public class EmailAndReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EmailOptions _options;
    private readonly ILogger<EmailAndReminderBackgroundService> _logger;

    public EmailAndReminderBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<EmailOptions> options,
        ILogger<EmailAndReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailAndReminderBackgroundService started. Scheduler Enabled: {Enabled}, Interval: {Interval}m",
            _options.EnableBackgroundScheduler, _options.ReminderCheckIntervalMinutes);

        // Initial delay on startup to allow database initialization to complete cleanly
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        var lastReminderRun = DateTime.MinValue;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                // 1. Process pending email outbox items frequently (every loop cycle)
                await emailService.ProcessPendingOutboxAsync(batchSize: 15, stoppingToken);

                // 2. Run fee reminder checks at configurable schedule (e.g., hourly or once per check interval)
                if (_options.EnableBackgroundScheduler && (DateTime.UtcNow - lastReminderRun).TotalMinutes >= _options.ReminderCheckIntervalMinutes)
                {
                    _logger.LogInformation("Executing scheduled fee reminders background job at {Time} UTC...", DateTime.UtcNow);
                    var reminderService = scope.ServiceProvider.GetRequiredService<IFeeReminderService>();
                    var processed = await reminderService.ProcessUpcomingAndOverdueRemindersAsync(stoppingToken);
                    lastReminderRun = DateTime.UtcNow;
                    _logger.LogInformation("Scheduled fee reminders completed. Processed {Count} reminders.", processed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during background email or reminder processing cycle.");
            }

            // Sleep between cycles (30 seconds for responsive outbox dispatching)
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("EmailAndReminderBackgroundService stopped.");
    }
}

