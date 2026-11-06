using StudentManagement.Api.DTOs.Common;
using StudentManagement.Api.DTOs.Staff;

namespace StudentManagement.Api.Services.Interfaces;

public interface IStaffService
{
    Task<PagedResultDto<StaffDto>> GetPagedStaffAsync(StaffQueryParameters query);
    Task<StaffDto?> GetStaffByIdAsync(int id);
    Task<StaffDto> CreateStaffAsync(CreateStaffDto dto);
    Task<StaffDto?> UpdateStaffAsync(int id, UpdateStaffDto dto);
    Task<bool?> ToggleStaffStatusAsync(int id);
    Task<List<string>> GetDepartmentsAsync();
}

