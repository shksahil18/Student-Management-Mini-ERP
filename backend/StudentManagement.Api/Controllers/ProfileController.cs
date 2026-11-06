using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Api.DTOs.Common;
using StudentManagement.Api.DTOs.Profile;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IWebHostEnvironment _env;

    public ProfileController(IProfileService profileService, IWebHostEnvironment env)
    {
        _profileService = profileService;
        _env = env;
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claim, out var userId)) return userId;
        return null;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<UserProfileDto>>> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized(ApiResponseDto<UserProfileDto>.Fail("Unauthorized access."));

        var profile = await _profileService.GetProfileAsync(userId.Value);
        if (profile == null)
        {
            return NotFound(ApiResponseDto<UserProfileDto>.Fail("User profile was not found."));
        }

        return Ok(ApiResponseDto<UserProfileDto>.Ok(profile));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponseDto<UserProfileDto>>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
        {
            var error = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Invalid profile update data.";
            return BadRequest(ApiResponseDto<UserProfileDto>.Fail(error));
        }

        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized(ApiResponseDto<UserProfileDto>.Fail("Unauthorized access."));

        var updated = await _profileService.UpdateProfileAsync(userId.Value, dto);
        if (updated == null)
        {
            return NotFound(ApiResponseDto<UserProfileDto>.Fail("User profile was not found."));
        }

        return Ok(ApiResponseDto<UserProfileDto>.Ok(updated, "Profile updated successfully."));
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponseDto<bool>>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            var error = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Invalid password data.";
            return BadRequest(ApiResponseDto<bool>.Fail(error));
        }

        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized(ApiResponseDto<bool>.Fail("Unauthorized access."));

        await _profileService.ChangePasswordAsync(userId.Value, dto);
        return Ok(ApiResponseDto<bool>.Ok(true, "Password changed successfully."));
    }

    [HttpPost("photo")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponseDto<string>>> UploadPhoto([FromForm] IFormFile file)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized(ApiResponseDto<string>.Fail("Unauthorized access."));

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var photoUrl = await _profileService.UploadProfilePhotoAsync(userId.Value, file, webRoot);

        return Ok(ApiResponseDto<string>.Ok(photoUrl, "Profile photo updated successfully."));
    }

    [HttpDelete("photo")]
    public async Task<ActionResult<ApiResponseDto<bool>>> RemovePhoto()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized(ApiResponseDto<bool>.Fail("Unauthorized access."));

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var success = await _profileService.RemoveProfilePhotoAsync(userId.Value, webRoot);

        return Ok(ApiResponseDto<bool>.Ok(success, "Profile photo removed successfully."));
    }
}

