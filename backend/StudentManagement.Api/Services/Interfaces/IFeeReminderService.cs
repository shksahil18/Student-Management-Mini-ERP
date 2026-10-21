namespace StudentManagement.Api.Services.Interfaces;

public interface IFeeReminderService
{
    Task<int> ProcessUpcomingAndOverdueRemindersAsync(CancellationToken cancellationToken = default);
}

