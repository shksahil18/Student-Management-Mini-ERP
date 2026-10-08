namespace StudentManagement.Api.Entities;

public class Payment
{
    public int Id { get; set; }
    public int FeeId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string PaymentMethod { get; set; } = "Cash"; // "Cash", "Card", "UPI", "Bank Transfer"
    public string? Notes { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Fee Fee { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}
