using Microsoft.EntityFrameworkCore;
using StudentManagement.Api.Data;
using StudentManagement.Api.DTOs.Common;
using StudentManagement.Api.DTOs.Staff;
using StudentManagement.Api.Entities;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Services.Implementations;

public class StaffService : IStaffService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StaffService> _logger;

    public StaffService(ApplicationDbContext context, ILogger<StaffService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResultDto<StaffDto>> GetPagedStaffAsync(StaffQueryParameters query)
    {
        var dbQuery = _context.Users.AsNoTracking().AsQueryable();

        // 1. Search filter across FullName, Username, Email, Designation, Department
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            dbQuery = dbQuery.Where(u =>
                (u.FullName != null && u.FullName.ToLower().Contains(search)) ||
                u.Username.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search) ||
                (u.Designation != null && u.Designation.ToLower().Contains(search)) ||
                (u.Department != null && u.Department.ToLower().Contains(search)));
        }

        // 2. Department filter
        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            var dept = query.Department.Trim().ToLower();
            dbQuery = dbQuery.Where(u => u.Department != null && u.Department.ToLower() == dept);
        }

        // 3. Status filter
        if (query.IsActive.HasValue)
        {
            dbQuery = dbQuery.Where(u => u.IsActive == query.IsActive.Value);
        }

        var totalItems = await dbQuery.CountAsync();

        var page = query.Page > 0 ? query.Page : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

        var items = await dbQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new StaffDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName ?? u.Username,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Designation = u.Designation ?? "Staff Member",
                Department = u.Department ?? "General",
                JoiningDate = u.JoiningDate ?? u.CreatedAt,
                Address = u.Address,
                ProfilePhotoUrl = u.ProfilePhotoUrl,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return new PagedResultDto<StaffDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<StaffDto?> GetStaffByIdAsync(int id)
    {
        var u = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return null;

        return new StaffDto
        {
            Id = u.Id,
            Username = u.Username,
            FullName = u.FullName ?? u.Username,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Designation = u.Designation ?? "Staff Member",
            Department = u.Department ?? "General",
            JoiningDate = u.JoiningDate ?? u.CreatedAt,
            Address = u.Address,
            ProfilePhotoUrl = u.ProfilePhotoUrl,
            Role = u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        };
    }

    public async Task<StaffDto> CreateStaffAsync(CreateStaffDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLower();
        var normalizedUsername = dto.Username.Trim().ToLower();

        // 1. Email uniqueness
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
        {
            throw new InvalidOperationException($"A user account with email '{dto.Email}' already exists.");
        }

        // 2. Username uniqueness
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername))
        {
            throw new InvalidOperationException($"A user account with username '{dto.Username}' already exists.");
        }

        var tempPassword = !string.IsNullOrWhiteSpace(dto.TemporaryPassword) ? dto.TemporaryPassword : "Staff@" + DateTime.UtcNow.Year;

        var user = new User
        {
            Username = dto.Username.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
            Role = "Staff", // Staff created via Staff Management are strictly Staff
            FullName = dto.FullName.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            Designation = dto.Designation.Trim(),
            Department = dto.Department.Trim(),
            JoiningDate = dto.JoiningDate ?? DateTime.UtcNow,
            Address = dto.Address?.Trim(),
            ProfilePhotoUrl = dto.ProfilePhotoUrl?.Trim(),
            ThemePreference = "system",
            EmailNotificationsEnabled = true,
            IsActive = dto.IsActive,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New staff account created: {Username} ({Email}), ID: {Id}", user.Username, user.Email, user.Id);

        return new StaffDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Designation = user.Designation,
            Department = user.Department,
            JoiningDate = user.JoiningDate,
            Address = user.Address,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<StaffDto?> UpdateStaffAsync(int id, UpdateStaffDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        user.FullName = dto.FullName.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.Designation = dto.Designation.Trim();
        user.Department = dto.Department.Trim();
        if (dto.JoiningDate.HasValue) user.JoiningDate = dto.JoiningDate.Value;
        user.Address = dto.Address?.Trim();
        user.ProfilePhotoUrl = dto.ProfilePhotoUrl?.Trim();
        user.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Staff account updated: ID {Id}", user.Id);

        return new StaffDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Designation = user.Designation,
            Department = user.Department,
            JoiningDate = user.JoiningDate,
            Address = user.Address,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool?> ToggleStaffStatusAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Staff status toggled for ID {Id}: IsActive = {IsActive}", user.Id, user.IsActive);

        return user.IsActive;
    }

    public async Task<List<string>> GetDepartmentsAsync()
    {
        var departments = await _context.Users
            .Where(u => !string.IsNullOrEmpty(u.Department))
            .Select(u => u.Department!)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        if (!departments.Contains("Admissions")) departments.Insert(0, "Admissions");
        if (!departments.Contains("Finance")) departments.Insert(1, "Finance");
        if (!departments.Contains("Academics")) departments.Insert(2, "Academics");
        if (!departments.Contains("Administration")) departments.Insert(3, "Administration");

        return departments.Distinct().OrderBy(d => d).ToList();
    }
}

