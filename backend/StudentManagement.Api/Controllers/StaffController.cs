using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Api.DTOs.Common;
using StudentManagement.Api.DTOs.Staff;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<StaffDto>>>> GetStaff([FromQuery] StaffQueryParameters query)
    {
        var result = await _staffService.GetPagedStaffAsync(query);
        return Ok(ApiResponseDto<PagedResultDto<StaffDto>>.Ok(result));
    }

    [HttpGet("departments")]
    public async Task<ActionResult<ApiResponseDto<List<string>>>> GetDepartments()
    {
        var departments = await _staffService.GetDepartmentsAsync();
        return Ok(ApiResponseDto<List<string>>.Ok(departments));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<StaffDto>>> GetStaffById(int id)
    {
        var staff = await _staffService.GetStaffByIdAsync(id);
        if (staff == null)
        {
            return NotFound(ApiResponseDto<StaffDto>.Fail($"Staff member with ID {id} was not found."));
        }

        return Ok(ApiResponseDto<StaffDto>.Ok(staff));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<StaffDto>>> CreateStaff([FromBody] CreateStaffDto dto)
    {
        if (!ModelState.IsValid)
        {
            var error = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Invalid staff member payload.";
            return BadRequest(ApiResponseDto<StaffDto>.Fail(error));
        }

        var staff = await _staffService.CreateStaffAsync(dto);
        return CreatedAtAction(nameof(GetStaffById), new { id = staff.Id },
            ApiResponseDto<StaffDto>.Ok(staff, "Staff member created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<StaffDto>>> UpdateStaff(int id, [FromBody] UpdateStaffDto dto)
    {
        if (!ModelState.IsValid)
        {
            var error = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Invalid staff member payload.";
            return BadRequest(ApiResponseDto<StaffDto>.Fail(error));
        }

        var updated = await _staffService.UpdateStaffAsync(id, dto);
        if (updated == null)
        {
            return NotFound(ApiResponseDto<StaffDto>.Fail($"Staff member with ID {id} was not found."));
        }

        return Ok(ApiResponseDto<StaffDto>.Ok(updated, "Staff member profile updated successfully."));
    }

    [HttpPatch("{id:int}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<bool>>> ToggleStatus(int id)
    {
        var newStatus = await _staffService.ToggleStaffStatusAsync(id);
        if (newStatus == null)
        {
            return NotFound(ApiResponseDto<bool>.Fail($"Staff member with ID {id} was not found."));
        }

        var message = newStatus.Value ? "Staff member account activated." : "Staff member account deactivated.";
        return Ok(ApiResponseDto<bool>.Ok(newStatus.Value, message));
    }
}

