using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Api.DTOs.Common;
using StudentManagement.Api.DTOs.Dashboard;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponseDto<DashboardSummaryDto>>> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return Ok(ApiResponseDto<DashboardSummaryDto>.Ok(summary));
    }
}
