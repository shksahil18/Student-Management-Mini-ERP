using Microsoft.EntityFrameworkCore;
using StudentManagement.Api.Data;
using StudentManagement.Api.DTOs.Fees;
using StudentManagement.Api.Entities;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Services.Implementations;

public class FeeService : IFeeService
{
    private readonly ApplicationDbContext _context;

    public FeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FeeDto>> GetFeesByStudentIdAsync(int studentId)
    {
        return await _context.Fees
            .AsNoTracking()
            .Where(f => f.StudentId == studentId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeeDto
            {
                Id = f.Id,
                StudentId = f.StudentId,
                TotalAmount = f.TotalAmount,
                PaidAmount = f.PaidAmount,
                OutstandingAmount = f.OutstandingAmount,
                DueDate = f.DueDate,
                Status = f.Status,
                CreatedAt = f.CreatedAt,
                Payments = f.Payments.OrderByDescending(p => p.PaymentDate).Select(p => new PaymentDto
                {
                    Id = p.Id,
                    FeeId = p.FeeId,
                    ReceiptNumber = p.ReceiptNumber,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    Notes = p.Notes,
                    CreatedByUserId = p.CreatedByUserId,
                    CreatedByUsername = p.CreatedByUser != null ? p.CreatedByUser.Username : "System"
                })
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<FeeDto>> GetAllFeesAsync()
    {
        return await _context.Fees
            .AsNoTracking()
            .Include(f => f.Student)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeeDto
            {
                Id = f.Id,
                StudentId = f.StudentId,
                TotalAmount = f.TotalAmount,
                PaidAmount = f.PaidAmount,
                OutstandingAmount = f.OutstandingAmount,
                DueDate = f.DueDate,
                Status = f.Status,
                CreatedAt = f.CreatedAt,
                Payments = f.Payments.OrderByDescending(p => p.PaymentDate).Select(p => new PaymentDto
                {
                    Id = p.Id,
                    FeeId = p.FeeId,
                    ReceiptNumber = p.ReceiptNumber,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    Notes = p.Notes,
                    CreatedByUserId = p.CreatedByUserId,
                    CreatedByUsername = p.CreatedByUser != null ? p.CreatedByUser.Username : "System"
                })
            })
            .ToListAsync();
    }

    public async Task<FeeDto> CreateFeeAsync(CreateFeeDto dto)
    {
        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null)
        {
            throw new KeyNotFoundException($"Student with ID {dto.StudentId} was not found.");
        }

        var fee = new Fee
        {
            StudentId = dto.StudentId,
            TotalAmount = dto.TotalAmount,
            PaidAmount = 0.00m,
            OutstandingAmount = dto.TotalAmount,
            DueDate = dto.DueDate,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Fees.AddAsync(fee);
        await _context.SaveChangesAsync();

        return new FeeDto
        {
            Id = fee.Id,
            StudentId = fee.StudentId,
            TotalAmount = fee.TotalAmount,
            PaidAmount = fee.PaidAmount,
            OutstandingAmount = fee.OutstandingAmount,
            DueDate = fee.DueDate,
            Status = fee.Status,
            CreatedAt = fee.CreatedAt
        };
    }

    public async Task<ReceiptDto> RecordPaymentAsync(int feeId, CreatePaymentDto dto, int currentUserId)
    {
        if (dto.Amount <= 0)
        {
            throw new ArgumentException("Payment amount must be strictly greater than 0.");
        }

        // 1. Begin atomic database transaction
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 2. Load fee record with tracking and include student
            var fee = await _context.Fees
                .Include(f => f.Student)
                .FirstOrDefaultAsync(f => f.Id == feeId);

            if (fee == null)
            {
                throw new KeyNotFoundException($"Fee record with ID {feeId} was not found.");
            }

            // 3. Verify outstanding balance & prevent overpayment
            if (dto.Amount > fee.OutstandingAmount)
            {
                throw new InvalidOperationException(
                    $"Payment amount of ₹{dto.Amount:N2} exceeds remaining outstanding balance of ₹{fee.OutstandingAmount:N2}.");
            }

            // 4. Update balances
            fee.PaidAmount += dto.Amount;
            fee.OutstandingAmount = fee.TotalAmount - fee.PaidAmount;
            fee.Status = fee.OutstandingAmount == 0 ? "Paid" : "Partial";
            fee.UpdatedAt = DateTime.UtcNow;

            // 5. Generate unique ReceiptNumber: REC-YYYY-XXXXXX
            var year = DateTime.UtcNow.Year;
            var paymentCount = await _context.Payments.CountAsync();
            var receiptNumber = $"REC-{year}-{(paymentCount + 1):D6}";

            while (await _context.Payments.AnyAsync(p => p.ReceiptNumber == receiptNumber))
            {
                paymentCount++;
                receiptNumber = $"REC-{year}-{(paymentCount + 1):D6}";
            }

            // 6. Create payment entity
            var payment = new Payment
            {
                FeeId = fee.Id,
                ReceiptNumber = receiptNumber,
                Amount = dto.Amount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = dto.PaymentMethod.Trim(),
                Notes = dto.Notes?.Trim(),
                CreatedByUserId = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            // 7. Commit database transaction
            await transaction.CommitAsync();

            var user = await _context.Users.FindAsync(currentUserId);

            return new ReceiptDto
            {
                ReceiptNumber = payment.ReceiptNumber,
                PaymentDate = payment.PaymentDate,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Notes = payment.Notes,
                RecordedBy = user?.Username ?? "Staff",
                StudentId = fee.Student.Id,
                StudentCode = fee.Student.StudentCode,
                StudentName = $"{fee.Student.FirstName} {fee.Student.LastName}".Trim(),
                Course = fee.Student.Course,
                FeeId = fee.Id,
                FeeTotalAmount = fee.TotalAmount,
                FeePaidAmount = fee.PaidAmount,
                RemainingOutstanding = fee.OutstandingAmount,
                FeeStatus = fee.Status
            };
        }
        catch
        {
            // Automatic rollback upon unhandled exception
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ReceiptDto?> GetReceiptByNumberAsync(string receiptNumber)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Fee)
                .ThenInclude(f => f.Student)
            .Include(p => p.CreatedByUser)
            .FirstOrDefaultAsync(p => p.ReceiptNumber == receiptNumber.Trim());

        if (payment == null) return null;

        var fee = payment.Fee;
        var student = fee.Student;

        return new ReceiptDto
        {
            ReceiptNumber = payment.ReceiptNumber,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            Notes = payment.Notes,
            RecordedBy = payment.CreatedByUser?.Username ?? "Staff",
            StudentId = student.Id,
            StudentCode = student.StudentCode,
            StudentName = $"{student.FirstName} {student.LastName}".Trim(),
            Course = student.Course,
            FeeId = fee.Id,
            FeeTotalAmount = fee.TotalAmount,
            FeePaidAmount = fee.PaidAmount,
            RemainingOutstanding = fee.OutstandingAmount,
            FeeStatus = fee.Status
        };
    }
}
