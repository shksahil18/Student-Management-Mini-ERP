using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Api.DTOs.Profile;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public DateTime? JoiningDate { get; set; }
    public string? Address { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string ThemePreference { get; set; } = "system"; // "light", "dark", "system"
    public bool EmailNotificationsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileDto
{
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    public string? FullName { get; set; }

    [Phone(ErrorMessage = "A valid phone number is required.")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Designation cannot exceed 100 characters.")]
    public string? Designation { get; set; }

    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
    public string? Department { get; set; }

    [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
    public string? Address { get; set; }

    [RegularExpression("^(light|dark|system)$", ErrorMessage = "Theme must be 'light', 'dark', or 'system'.")]
    public string? ThemePreference { get; set; }

    public bool? EmailNotificationsEnabled { get; set; }
}

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters long.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required.")]
    [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

