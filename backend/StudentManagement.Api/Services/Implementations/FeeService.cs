using Microsoft.EntityFrameworkCore;
using StudentManagement.Api.Data;
using StudentManagement.Api.DTOs.Fees;
using StudentManagement.Api.Entities;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Services.Implementations;

public class FeeService : IFeeService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly ILogger<FeeService> _logger;

    public FeeService(
        ApplicationDbContext context,
        IEmailService emailService,
        IEmailTemplateService emailTemplateService,
        ILogger<FeeService> logger)
    {
        _context = context;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
        _logger = logger;
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

        // Verify that the user recording the payment exists
        var user = await _context.Users.FindAsync(currentUserId);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Active user account was not found for this token. Please log in again.");
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
            if (fee.OutstandingAmount <= 0)
            {
                throw new InvalidOperationException("This fee record has already been fully settled/paid.");
            }

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

            // 5. Generate collision-proof unique ReceiptNumber: REC-YYYY-XXXXXX
            var year = DateTime.UtcNow.Year;
            var baseCode = $"REC-{year}";
            int attempt = 0;
            string receiptNumber;

            do
            {
                var paymentCount = await _context.Payments.CountAsync();
                receiptNumber = $"{baseCode}-{(paymentCount + 1 + attempt):D6}";
                attempt++;

                if (attempt > 5)
                {
                    receiptNumber = $"{baseCode}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                    break;
                }
            }
            while (await _context.Payments.AnyAsync(p => p.ReceiptNumber == receiptNumber));

            // 6. Create payment entity
            var payment = new Payment
            {
                FeeId = fee.Id,
                ReceiptNumber = receiptNumber,
                Amount = dto.Amount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? "Cash" : dto.PaymentMethod.Trim(),
                Notes = dto.Notes?.Trim(),
                CreatedByUserId = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            // 7. Commit database transaction
            await transaction.CommitAsync();

            var receiptDto = new ReceiptDto
            {
                ReceiptNumber = payment.ReceiptNumber,
                PaymentDate = payment.PaymentDate,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Notes = payment.Notes,
                RecordedBy = user.Username,
                StudentId = fee.Student?.Id ?? fee.StudentId,
                StudentCode = fee.Student?.StudentCode ?? string.Empty,
                StudentName = fee.Student != null ? $"{fee.Student.FirstName} {fee.Student.LastName}".Trim() : "Student",
                Course = fee.Student?.Course ?? string.Empty,
                FeeId = fee.Id,
                FeeTotalAmount = fee.TotalAmount,
                FeePaidAmount = fee.PaidAmount,
                RemainingOutstanding = fee.OutstandingAmount,
                FeeStatus = fee.Status
            };

            // Queue receipt email only after successful database commit
            try
            {
                if (!string.IsNullOrWhiteSpace(fee.Student?.Email))
                {
                    var emailMsg = _emailTemplateService.BuildFeeReceiptEmail(receiptDto, fee.Student.Email);
                    await _emailService.QueueEmailAsync(emailMsg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue receipt email for {ReceiptNumber}", receiptDto.ReceiptNumber);
            }

            return receiptDto;
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
