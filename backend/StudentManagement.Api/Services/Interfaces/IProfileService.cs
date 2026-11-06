using Microsoft.AspNetCore.Http;
using StudentManagement.Api.DTOs.Profile;

namespace StudentManagement.Api.Services.Interfaces;

public interface IProfileService
{
    Task<UserProfileDto?> GetProfileAsync(int userId);
    Task<UserProfileDto?> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<string> UploadProfilePhotoAsync(int userId, IFormFile file, string webRootPath);
    Task<bool> RemoveProfilePhotoAsync(int userId, string webRootPath);
}

