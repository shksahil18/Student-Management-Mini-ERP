using Microsoft.EntityFrameworkCore;
using StudentManagement.Api.Data;
using StudentManagement.Api.DTOs.Dashboard;
using StudentManagement.Api.DTOs.Fees;
using StudentManagement.Api.DTOs.Students;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        // 1. Student counts
        var totalStudents = await _context.Students.CountAsync();
        var activeStudents = await _context.Students.CountAsync(s => s.IsActive);
        var inactiveStudents = totalStudents - activeStudents;

        // 2. Fee aggregates
        var totalBilled = await _context.Fees.SumAsync(f => (decimal?)f.TotalAmount) ?? 0m;
        var totalCollected = await _context.Fees.SumAsync(f => (decimal?)f.PaidAmount) ?? 0m;
        var totalOutstanding = await _context.Fees.SumAsync(f => (decimal?)f.OutstandingAmount) ?? 0m;

        // 3. Recent 5 Payments
        var recentPayments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.CreatedByUser)
            .OrderByDescending(p => p.PaymentDate)
            .Take(5)
            .Select(p => new PaymentDto
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
            .ToListAsync();

        // 4. Recent 5 Admissions
        var recentAdmissions = await _context.Students
            .AsNoTracking()
            .OrderByDescending(s => s.AdmissionDate)
            .Take(5)
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

        return new DashboardSummaryDto
        {
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            InactiveStudents = inactiveStudents,
            TotalFeesBilled = totalBilled,
            TotalFeesCollected = totalCollected,
            TotalOutstandingFees = totalOutstanding,
            RecentPayments = recentPayments,
            RecentAdmissions = recentAdmissions
        };
    }
}
