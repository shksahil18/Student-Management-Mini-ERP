namespace StudentManagement.Api.Services.Interfaces;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 25;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = false;
    public string SenderEmail { get; set; } = "noreply@erp.institution.edu";
    public string SenderName { get; set; } = "Known Student Management ERP";
    public string InstitutionName { get; set; } = "Known Institute of Technology";
    public string SupportContact { get; set; } = "support@institution.edu";
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
    public int VerificationTokenExpiryHours { get; set; } = 24;
    public int PasswordResetTokenExpiryMinutes { get; set; } = 60;
    public int UpcomingReminderLeadDays { get; set; } = 3;
    public int OverdueReminderCooldownDays { get; set; } = 7;
    public int ReminderCheckIntervalMinutes { get; set; } = 60;
    public bool EnableBackgroundScheduler { get; set; } = true;
}

