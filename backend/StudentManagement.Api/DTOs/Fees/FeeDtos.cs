using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Api.DTOs.Fees;

public class FeeDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IEnumerable<PaymentDto> Payments { get; set; } = new List<PaymentDto>();
}

public class CreateFeeDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [Range(0.01, 10000000.00, ErrorMessage = "Total fee amount must be greater than zero.")]
    public decimal TotalAmount { get; set; }

    [Required]
    public DateTime DueDate { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }
    public int FeeId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
}

public class CreatePaymentDto
{
    [Required]
    [Range(0.01, 10000000.00, ErrorMessage = "Payment amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Payment method is required.")]
    [StringLength(30)]
    public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, UPI, Bank Transfer

    [StringLength(255)]
    public string? Notes { get; set; }
}

public class ReceiptDto
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string RecordedBy { get; set; } = string.Empty;

    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;

    public int FeeId { get; set; }
    public decimal FeeTotalAmount { get; set; }
    public decimal FeePaidAmount { get; set; }
    public decimal RemainingOutstanding { get; set; }
    public string FeeStatus { get; set; } = string.Empty;
}
