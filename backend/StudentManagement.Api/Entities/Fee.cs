namespace StudentManagement.Api.Entities;

public class Fee
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Pending"; // "Pending", "Partial", "Paid"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Student Student { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
