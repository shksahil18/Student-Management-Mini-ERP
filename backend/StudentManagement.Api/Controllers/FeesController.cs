using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Api.DTOs.Common;
using StudentManagement.Api.DTOs.Fees;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeesController : ControllerBase
{
    private readonly IFeeService _feeService;

    public FeesController(IFeeService feeService)
    {
        _feeService = feeService;
    }

    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<FeeDto>>>> GetFeesByStudent(int studentId)
    {
        var fees = await _feeService.GetFeesByStudentIdAsync(studentId);
        return Ok(ApiResponseDto<IEnumerable<FeeDto>>.Ok(fees));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<FeeDto>>>> GetAllFees()
    {
        var fees = await _feeService.GetAllFeesAsync();
        return Ok(ApiResponseDto<IEnumerable<FeeDto>>.Ok(fees));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<FeeDto>>> CreateFee([FromBody] CreateFeeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<FeeDto>.Fail("Invalid submission data."));
        }

        var fee = await _feeService.CreateFeeAsync(dto);
        return CreatedAtAction(nameof(GetFeesByStudent), new { studentId = fee.StudentId },
            ApiResponseDto<FeeDto>.Ok(fee, "Fee structure assigned successfully."));
    }

    [HttpPost("{feeId:int}/payments")]
    public async Task<ActionResult<ApiResponseDto<ReceiptDto>>> RecordPayment(int feeId, [FromBody] CreatePaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<ReceiptDto>.Fail("Invalid submission data."));
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized(ApiResponseDto<ReceiptDto>.Fail("Invalid user token."));
        }

        var receipt = await _feeService.RecordPaymentAsync(feeId, dto, currentUserId);
        return CreatedAtAction(nameof(GetReceipt), new { receiptNumber = receipt.ReceiptNumber },
            ApiResponseDto<ReceiptDto>.Ok(receipt, "Payment recorded successfully."));
    }

    [HttpGet("receipts/{receiptNumber}")]
    public async Task<ActionResult<ApiResponseDto<ReceiptDto>>> GetReceipt(string receiptNumber)
    {
        var receipt = await _feeService.GetReceiptByNumberAsync(receiptNumber);
        if (receipt == null)
        {
            return NotFound(ApiResponseDto<ReceiptDto>.Fail($"Receipt '{receiptNumber}' was not found."));
        }

        return Ok(ApiResponseDto<ReceiptDto>.Ok(receipt));
    }

    [HttpPost("trigger-reminders")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<int>>> TriggerFeeReminders([FromServices] IFeeReminderService reminderService)
    {
        var count = await reminderService.ProcessUpcomingAndOverdueRemindersAsync();
        return Ok(ApiResponseDto<int>.Ok(count, $"Successfully evaluated fee reminders. {count} reminder email(s) queued."));
    }
}
