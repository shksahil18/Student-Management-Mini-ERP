using StudentManagement.Api.DTOs.Fees;
using StudentManagement.Api.DTOs.Students;

namespace StudentManagement.Api.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int InactiveStudents { get; set; }

    public decimal TotalFeesBilled { get; set; }
    public decimal TotalFeesCollected { get; set; }
    public decimal TotalOutstandingFees { get; set; }

    public IEnumerable<PaymentDto> RecentPayments { get; set; } = new List<PaymentDto>();
    public IEnumerable<StudentDto> RecentAdmissions { get; set; } = new List<StudentDto>();
}
