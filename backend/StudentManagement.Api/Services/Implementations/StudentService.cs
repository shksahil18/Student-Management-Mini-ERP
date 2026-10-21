using System.Text;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Api.Data;
using StudentManagement.Api.DTOs.Common;
using StudentManagement.Api.DTOs.Fees;
using StudentManagement.Api.DTOs.Students;
using StudentManagement.Api.Entities;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        ApplicationDbContext context,
        IEmailService emailService,
        IEmailTemplateService emailTemplateService,
        ILogger<StudentService> logger)
    {
        _context = context;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
        _logger = logger;
    }

    public async Task<PagedResultDto<StudentDto>> GetPagedStudentsAsync(StudentQueryParameters query)
    {
        var dbQuery = _context.Students.AsNoTracking().AsQueryable();

        // 1. Search filter across StudentCode, FirstName, LastName, Email
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            dbQuery = dbQuery.Where(s =>
                s.StudentCode.ToLower().Contains(search) ||
                s.FirstName.ToLower().Contains(search) ||
                s.LastName.ToLower().Contains(search) ||
                s.Email.ToLower().Contains(search));
        }

        // 2. Course filter
        if (!string.IsNullOrWhiteSpace(query.Course))
        {
            dbQuery = dbQuery.Where(s => s.Course == query.Course.Trim());
        }

        // 3. Status filter
        if (query.IsActive.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.IsActive == query.IsActive.Value);
        }

        var totalItems = await dbQuery.CountAsync();

        var page = query.Page > 0 ? query.Page : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

        var items = await dbQuery
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                StudentCode = s.StudentCode,
                FirstName = s.FirstName,
                LastName = s.LastName,
                DateOfBirth = s.DateOfBirth,
                Gender = s.Gender,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                AdmissionDate = s.AdmissionDate,
                Course = s.Course,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return new PagedResultDto<StudentDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<StudentDetailDto?> GetStudentByIdAsync(int id)
    {
        var student = await _context.Students
            .AsNoTracking()
            .Include(s => s.Fees)
                .ThenInclude(f => f.Payments)
                    .ThenInclude(p => p.CreatedByUser)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null) return null;

        return new StudentDetailDto
        {
            Id = student.Id,
            StudentCode = student.StudentCode,
            FirstName = student.FirstName,
            LastName = student.LastName,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            Email = student.Email,
            Phone = student.Phone,
            Address = student.Address,
            AdmissionDate = student.AdmissionDate,
            Course = student.Course,
            IsActive = student.IsActive,
            CreatedAt = student.CreatedAt,
            Fees = student.Fees.Select(f => new FeeDto
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
                    CreatedByUsername = p.CreatedByUser?.Username ?? "System"
                })
            })
        };
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentDto dto)
    {
        // 1. Validate email uniqueness
        var emailExists = await _context.Students.AnyAsync(s => s.Email.ToLower() == dto.Email.Trim().ToLower());
        if (emailExists)
        {
            throw new InvalidOperationException($"Student with email '{dto.Email}' already exists.");
        }

        // 2. Generate unique StudentCode (e.g., STU-2026-00007)
        var year = DateTime.UtcNow.Year;
        var count = await _context.Students.CountAsync();
        var studentCode = $"STU-{year}-{(count + 1):D5}";

        // Ensure no collision
        while (await _context.Students.AnyAsync(s => s.StudentCode == studentCode))
        {
            count++;
            studentCode = $"STU-{year}-{(count + 1):D5}";
        }

        var student = new Student
        {
            StudentCode = studentCode,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone.Trim(),
            Address = dto.Address?.Trim(),
            AdmissionDate = dto.AdmissionDate,
            Course = dto.Course.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();

        var resultDto = new StudentDto
        {
            Id = student.Id,
            StudentCode = student.StudentCode,
            FirstName = student.FirstName,
            LastName = student.LastName,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            Email = student.Email,
            Phone = student.Phone,
            Address = student.Address,
            AdmissionDate = student.AdmissionDate,
            Course = student.Course,
            IsActive = student.IsActive,
            CreatedAt = student.CreatedAt
        };

        // Queue admission confirmation email asynchronously (only after successful DB commit)
        try
        {
            var emailMsg = _emailTemplateService.BuildAdmissionConfirmationEmail(resultDto);
            await _emailService.QueueEmailAsync(emailMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue admission email for {StudentCode}", student.StudentCode);
        }

        return resultDto;
    }

    public async Task<StudentDto?> UpdateStudentAsync(int id, UpdateStudentDto dto)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return null;

        // Check if new email conflicts with another student
        var emailConflict = await _context.Students
            .AnyAsync(s => s.Email.ToLower() == dto.Email.Trim().ToLower() && s.Id != id);

        if (emailConflict)
        {
            throw new InvalidOperationException($"Email '{dto.Email}' is already in use by another student.");
        }

        student.FirstName = dto.FirstName.Trim();
        student.LastName = dto.LastName.Trim();
        student.DateOfBirth = dto.DateOfBirth;
        student.Gender = dto.Gender.Trim();
        student.Email = dto.Email.Trim();
        student.Phone = dto.Phone.Trim();
        student.Address = dto.Address?.Trim();
        student.AdmissionDate = dto.AdmissionDate;
        student.Course = dto.Course.Trim();
        student.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new StudentDto
        {
            Id = student.Id,
            StudentCode = student.StudentCode,
            FirstName = student.FirstName,
            LastName = student.LastName,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            Email = student.Email,
            Phone = student.Phone,
            Address = student.Address,
            AdmissionDate = student.AdmissionDate,
            Course = student.Course,
            IsActive = student.IsActive,
            CreatedAt = student.CreatedAt
        };
    }

    public async Task<bool?> ToggleStudentStatusAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return null;

        student.IsActive = !student.IsActive;
        student.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return student.IsActive;
    }

    public async Task<byte[]> ExportStudentsToCsvAsync()
    {
        var students = await _context.Students
            .AsNoTracking()
            .OrderBy(s => s.StudentCode)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("StudentCode,FirstName,LastName,Email,Phone,Course,DateOfBirth,AdmissionDate,Status");

        foreach (var s in students)
        {
            var status = s.IsActive ? "Active" : "Inactive";
            var cleanAddress = (s.Address ?? "").Replace(",", " ");
            sb.AppendLine($"\"{s.StudentCode}\",\"{s.FirstName}\",\"{s.LastName}\",\"{s.Email}\",\"{s.Phone}\",\"{s.Course}\",\"{s.DateOfBirth:yyyy-MM-dd}\",\"{s.AdmissionDate:yyyy-MM-dd}\",\"{status}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
