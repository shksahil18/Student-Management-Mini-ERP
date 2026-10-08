using System.ComponentModel.DataAnnotations;
using StudentManagement.Api.DTOs.Fees;

namespace StudentManagement.Api.DTOs.Students;

public class StudentDto
{
    public int Id { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime AdmissionDate { get; set; }
    public string Course { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStudentDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    [Phone(ErrorMessage = "Invalid phone format")]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Admission date is required")]
    public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Course is required")]
    [StringLength(50)]
    public string Course { get; set; } = string.Empty;
}

public class UpdateStudentDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    [Phone(ErrorMessage = "Invalid phone format")]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Admission date is required")]
    public DateTime AdmissionDate { get; set; }

    [Required(ErrorMessage = "Course is required")]
    [StringLength(50)]
    public string Course { get; set; } = string.Empty;
}

public class StudentDetailDto : StudentDto
{
    public IEnumerable<FeeDto> Fees { get; set; } = new List<FeeDto>();
    public decimal TotalBilled => Fees.Sum(f => f.TotalAmount);
    public decimal TotalPaid => Fees.Sum(f => f.PaidAmount);
    public decimal TotalOutstanding => Fees.Sum(f => f.OutstandingAmount);
}

public class StudentQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Course { get; set; }
    public bool? IsActive { get; set; }
}
