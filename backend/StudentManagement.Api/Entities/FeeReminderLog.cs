namespace StudentManagement.Api.Entities;

public enum ReminderType
{
    Upcoming = 1,
    Overdue = 2
}

public class FeeReminderLog
{
    public int Id { get; set; }
    public int FeeId { get; set; }
    public int StudentId { get; set; }
    public ReminderType ReminderType { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string RecipientEmail { get; set; } = string.Empty;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }

    // Navigation
    public Fee Fee { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

