using StudentManagement.Api.DTOs.Fees;

namespace StudentManagement.Api.Services.Interfaces;

public interface IFeeService
{
    Task<IEnumerable<FeeDto>> GetFeesByStudentIdAsync(int studentId);
    Task<IEnumerable<FeeDto>> GetAllFeesAsync();
    Task<FeeDto> CreateFeeAsync(CreateFeeDto dto);
    Task<ReceiptDto> RecordPaymentAsync(int feeId, CreatePaymentDto dto, int currentUserId);
    Task<ReceiptDto?> GetReceiptByNumberAsync(string receiptNumber);
}
