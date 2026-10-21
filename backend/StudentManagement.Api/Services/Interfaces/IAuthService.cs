using StudentManagement.Api.DTOs.Auth;
using StudentManagement.Api.DTOs.Common;

namespace StudentManagement.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> AuthenticateAsync(LoginRequestDto request);
    Task<UserDto?> GetCurrentUserAsync(int userId);
    Task<ApiResponseDto<UserDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponseDto<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<ApiResponseDto<bool>> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<ApiResponseDto<bool>> VerifyEmailAsync(VerifyEmailRequestDto request);
    Task<ApiResponseDto<bool>> ResendVerificationAsync(ResendVerificationRequestDto request);
}

