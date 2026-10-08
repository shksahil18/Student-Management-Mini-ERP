using StudentManagement.Api.DTOs.Common;
using StudentManagement.Api.DTOs.Students;

namespace StudentManagement.Api.Services.Interfaces;

public interface IStudentService
{
    Task<PagedResultDto<StudentDto>> GetPagedStudentsAsync(StudentQueryParameters query);
    Task<StudentDetailDto?> GetStudentByIdAsync(int id);
    Task<StudentDto> CreateStudentAsync(CreateStudentDto dto);
    Task<StudentDto?> UpdateStudentAsync(int id, UpdateStudentDto dto);
    Task<bool?> ToggleStudentStatusAsync(int id);
    Task<byte[]> ExportStudentsToCsvAsync();
}
