using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Api.Data;
using StudentManagement.Api.DTOs.Profile;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Services.Implementations;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(ApplicationDbContext context, ILogger<ProfileService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || !user.IsActive) return null;

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            FullName = user.FullName ?? user.Username,
            PhoneNumber = user.PhoneNumber,
            Designation = user.Designation ?? "Staff Member",
            Department = user.Department ?? "General",
            JoiningDate = user.JoiningDate ?? user.CreatedAt,
            Address = user.Address,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            ThemePreference = user.ThemePreference ?? "system",
            EmailNotificationsEnabled = user.EmailNotificationsEnabled,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.IsActive) return null;

        if (dto.FullName != null) user.FullName = dto.FullName.Trim();
        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber.Trim();
        if (dto.Designation != null) user.Designation = dto.Designation.Trim();
        if (dto.Department != null) user.Department = dto.Department.Trim();
        if (dto.Address != null) user.Address = dto.Address.Trim();
        if (!string.IsNullOrWhiteSpace(dto.ThemePreference)) user.ThemePreference = dto.ThemePreference.Trim().ToLowerInvariant();
        if (dto.EmailNotificationsEnabled.HasValue) user.EmailNotificationsEnabled = dto.EmailNotificationsEnabled.Value;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Profile updated for user ID: {UserId}", userId);

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Designation = user.Designation,
            Department = user.Department,
            JoiningDate = user.JoiningDate,
            Address = user.Address,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            ThemePreference = user.ThemePreference ?? "system",
            EmailNotificationsEnabled = user.EmailNotificationsEnabled,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.IsActive)
        {
            throw new KeyNotFoundException("User account not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password provided is incorrect.");
        }

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            throw new InvalidOperationException("New password and confirmation do not match.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Password changed successfully for user ID {UserId}", userId);
        return true;
    }

    public async Task<string> UploadProfilePhotoAsync(int userId, IFormFile file, string webRootPath)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No image file was uploaded.");
        }

        // Limit file size to 2MB
        if (file.Length > 2 * 1024 * 1024)
        {
            throw new InvalidOperationException("Profile photo file size cannot exceed 2 MB.");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(ext))
        {
            throw new InvalidOperationException("Invalid image format. Only JPG, JPEG, PNG, and WebP are accepted.");
        }

        // Validate actual file magic bytes to prevent masquerading
        using (var stream = file.OpenReadStream())
        {
            var header = new byte[8];
            var bytesRead = await stream.ReadAsync(header.AsMemory(0, 8));
            if (bytesRead < 4)
            {
                throw new InvalidOperationException("Corrupted or invalid image file.");
            }

            bool isValidImage = (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) // JPEG
                             || (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) // PNG
                             || (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46); // WEBP / RIFF

            if (!isValidImage)
            {
                throw new InvalidOperationException("Uploaded file does not match genuine image content.");
            }
        }

        var targetFolder = Path.Combine(webRootPath, "uploads", "profiles");
        Directory.CreateDirectory(targetFolder);

        var safeFileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(targetFolder, safeFileName);

        using (var destinationStream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(destinationStream);
        }

        var relativeUrl = $"/uploads/profiles/{safeFileName}";

        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            // Remove previous file if exists locally
            if (!string.IsNullOrWhiteSpace(user.ProfilePhotoUrl) && user.ProfilePhotoUrl.StartsWith("/uploads/profiles/"))
            {
                try
                {
                    var oldFilePath = Path.Combine(webRootPath, user.ProfilePhotoUrl.TrimStart('/'));
                    if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clean up old profile image");
                }
            }

            user.ProfilePhotoUrl = relativeUrl;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Profile photo updated for user ID {UserId}: {Url}", userId, relativeUrl);
        return relativeUrl;
    }

    public async Task<bool> RemoveProfilePhotoAsync(int userId, string webRootPath)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        if (!string.IsNullOrWhiteSpace(user.ProfilePhotoUrl) && user.ProfilePhotoUrl.StartsWith("/uploads/profiles/"))
        {
            try
            {
                var oldFilePath = Path.Combine(webRootPath, user.ProfilePhotoUrl.TrimStart('/'));
                if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up old profile image file");
            }
        }

        user.ProfilePhotoUrl = null;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Profile photo removed for user ID {UserId}", userId);
        return true;
    }
}

