
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Application.Interfaces;

namespace SchoolAdmission.API.Controllers;

[ApiController]
[Route("holds")]
public class HoldsController : ControllerBase
{
    private readonly IAdmissionService _service;

    public HoldsController(IAdmissionService service)
    {
        _service = service;
    }

    // GET /holds/{holdId}
    [HttpGet("{holdId:guid}")]
    public async Task<IActionResult> GetHold(Guid holdId)
    {
        var studentId = GetStudentId();

        if (studentId is null)
            return BadRequest(new { message = "X-Student-Id header is required." });

        var result = await _service.GetHoldAsync(holdId, studentId);
        return Ok(result);
    }

    // POST /holds/{holdId}/confirm
    [HttpPost("{holdId:guid}/confirm")]
    public async Task<IActionResult> ConfirmHold(Guid holdId)
    {
        var studentId = GetStudentId();

        if (studentId is null)
            return BadRequest(new { message = "X-Student-Id header is required." });

        var result = await _service.ConfirmHoldAsync(holdId, studentId);
        return Ok(result);
    }

    // DELETE /holds/{holdId}
    [HttpDelete("{holdId:guid}")]
    public async Task<IActionResult> CancelHold(Guid holdId)
    {
        var studentId = GetStudentId();

        if (studentId is null)
            return BadRequest(new { message = "X-Student-Id header is required." });

        await _service.CancelHoldAsync(holdId, studentId);
        return NoContent();
    }

    // GET /me/holds is implemented in this controller
    [HttpGet("/me/holds")]
    public async Task<IActionResult> GetMyHolds()
    {
        var studentId = GetStudentId();

        if (studentId is null)
            return BadRequest(new { message = "X-Student-Id header is required." });

        var result = await _service.GetMyHoldsAsync(studentId);
        return Ok(result);
    }

    private string? GetStudentId()
    {
        var studentId = Request.Headers["X-Student-Id"].ToString();
        return string.IsNullOrWhiteSpace(studentId) ? null : studentId;
    }
}
