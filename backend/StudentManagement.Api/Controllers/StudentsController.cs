using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Api.DTOs.Common;
using StudentManagement.Api.DTOs.Students;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<StudentDto>>>> GetStudents([FromQuery] StudentQueryParameters query)
    {
        var result = await _studentService.GetPagedStudentsAsync(query);
        return Ok(ApiResponseDto<PagedResultDto<StudentDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<StudentDetailDto>>> GetStudentById(int id)
    {
        var student = await _studentService.GetStudentByIdAsync(id);
        if (student == null)
        {
            return NotFound(ApiResponseDto<StudentDetailDto>.Fail($"Student with ID {id} was not found."));
        }

        return Ok(ApiResponseDto<StudentDetailDto>.Ok(student));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<StudentDto>>> CreateStudent([FromBody] CreateStudentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<StudentDto>.Fail("Invalid submission data."));
        }

        var student = await _studentService.CreateStudentAsync(dto);
        return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, 
            ApiResponseDto<StudentDto>.Ok(student, "Student admitted successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<StudentDto>>> UpdateStudent(int id, [FromBody] UpdateStudentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<StudentDto>.Fail("Invalid submission data."));
        }

        var updated = await _studentService.UpdateStudentAsync(id, dto);
        if (updated == null)
        {
            return NotFound(ApiResponseDto<StudentDto>.Fail($"Student with ID {id} was not found."));
        }

        return Ok(ApiResponseDto<StudentDto>.Ok(updated, "Student profile updated successfully."));
    }

    [HttpPatch("{id:int}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<bool>>> ToggleStatus(int id)
    {
        var newStatus = await _studentService.ToggleStudentStatusAsync(id);
        if (newStatus == null)
        {
            return NotFound(ApiResponseDto<bool>.Fail($"Student with ID {id} was not found."));
        }

        var message = newStatus.Value ? "Student activated successfully." : "Student deactivated successfully.";
        return Ok(ApiResponseDto<bool>.Ok(newStatus.Value, message));
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportToCsv()
    {
        var csvBytes = await _studentService.ExportStudentsToCsvAsync();
        var fileName = $"students_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(csvBytes, "text/csv", fileName);
    }
}
