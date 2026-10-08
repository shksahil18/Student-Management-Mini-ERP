using StudentManagement.Api.DTOs.Auth;

namespace StudentManagement.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> AuthenticateAsync(LoginRequestDto request);
    Task<UserDto?> GetCurrentUserAsync(int userId);
}
