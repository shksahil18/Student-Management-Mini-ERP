using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Api.DTOs.Staff;

public class StaffDto
{
    public int Id { get; set; }
    public string StaffId => $"STF-{Id:D4}";
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public DateTime? JoiningDate { get; set; }
    public string? Address { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string Role { get; set; } = "Staff";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStaffDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "A valid phone number is required.")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Designation is required.")]
    [StringLength(100, ErrorMessage = "Designation cannot exceed 100 characters.")]
    public string Designation { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
    public string Department { get; set; } = string.Empty;

    public DateTime? JoiningDate { get; set; }

    [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
    public string? Address { get; set; }

    [StringLength(1000, ErrorMessage = "Profile photo URL cannot exceed 1000 characters.")]
    public string? ProfilePhotoUrl { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Temporary password must be at least 6 characters long.")]
    public string? TemporaryPassword { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateStaffDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "A valid phone number is required.")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Designation is required.")]
    [StringLength(100, ErrorMessage = "Designation cannot exceed 100 characters.")]
    public string Designation { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
    public string Department { get; set; } = string.Empty;

    public DateTime? JoiningDate { get; set; }

    [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
    public string? Address { get; set; }

    [StringLength(1000, ErrorMessage = "Profile photo URL cannot exceed 1000 characters.")]
    public string? ProfilePhotoUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

public class StaffQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Department { get; set; }
    public bool? IsActive { get; set; }
}

