namespace StudentManagement.Api.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Staff"; // "Admin" or "Staff"
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = true; // Defaults to true for backward compatibility with seeded users
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Staff & Profile Extensions
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public DateTime? JoiningDate { get; set; }
    public string? Address { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string ThemePreference { get; set; } = "system"; // "light", "dark", "system"
    public bool EmailNotificationsEnabled { get; set; } = true;

    // Navigation properties
    public ICollection<Payment> RecordedPayments { get; set; } = new List<Payment>();
    public ICollection<EmailVerificationToken> VerificationTokens { get; set; } = new List<EmailVerificationToken>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}

