using StudentManagement.Api.DTOs.Auth;
using StudentManagement.Api.DTOs.Common;

namespace StudentManagement.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> AuthenticateAsync(LoginRequestDto request);
    Task<UserDto?> GetCurrentUserAsync(int userId);
    Task<ApiResponseDto<UserDto>> RegisterAsync(RegisterRequestDto request);
}

