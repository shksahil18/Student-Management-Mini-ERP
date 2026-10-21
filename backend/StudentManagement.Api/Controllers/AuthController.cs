using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Api.DTOs.Auth;
using StudentManagement.Api.DTOs.Common;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponseDto<AuthResponseDto>.Fail("Username and password are required."));
        }

        var result = await _authService.AuthenticateAsync(request);
        if (result == null)
        {
            return Unauthorized(ApiResponseDto<AuthResponseDto>.Fail("Invalid credentials or account deactivated."));
        }

        return Ok(ApiResponseDto<AuthResponseDto>.Ok(result, "Login successful."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponseDto<UserDto>.Fail("Invalid user token."));
        }

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponseDto<UserDto>.Fail("User account not found."));
        }

        return Ok(ApiResponseDto<UserDto>.Ok(user));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var error = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Invalid registration payload.";
            return BadRequest(ApiResponseDto<UserDto>.Fail(error));
        }

        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
        {
            return Conflict(result);
        }

        return StatusCode(201, result);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<bool>>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<bool>.Fail("A valid email address is required."));
        }

        var result = await _authService.ForgotPasswordAsync(request);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<bool>>> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var error = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Invalid password reset data.";
            return BadRequest(ApiResponseDto<bool>.Fail(error));
        }

        var result = await _authService.ResetPasswordAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<bool>>> VerifyEmail([FromBody] VerifyEmailRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(ApiResponseDto<bool>.Fail("Verification token is required."));
        }

        var result = await _authService.VerifyEmailAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<bool>>> ResendVerification([FromBody] ResendVerificationRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<bool>.Fail("A valid email address is required."));
        }

        var result = await _authService.ResendVerificationAsync(request);
        return Ok(result);
    }
}


