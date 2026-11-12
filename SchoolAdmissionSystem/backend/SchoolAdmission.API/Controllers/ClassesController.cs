
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Application.DTOs;
using SchoolAdmission.Application.Interfaces;

namespace SchoolAdmission.API.Controllers;

[ApiController]
[Route("classes")]
public class ClassesController : ControllerBase
{
    private readonly IAdmissionService _service;

    public ClassesController(IAdmissionService service)
    {
        _service = service;
    }

    // POST /classes
    [HttpPost]
    public async Task<IActionResult> CreateClass(
        [FromBody] CreateClassRequest request)
    {
        var result = await _service.CreateClassAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // GET /classes
    [HttpGet]
    public async Task<IActionResult> GetClasses()
    {
        var result = await _service.GetClassesAsync();
        return Ok(result);
    }

    // GET /classes/{classId}
    [HttpGet("{classId:guid}")]
    public async Task<IActionResult> GetClass(Guid classId)
    {
        var result = await _service.GetClassAsync(classId);
        return Ok(result);
    }

    // POST /classes/{classId}/holds
    [HttpPost("{classId:guid}/holds")]
    public async Task<IActionResult> CreateHold(Guid classId)
    {
        var studentId = GetStudentId();

        if (studentId is null)
            return BadRequest(new { message = "X-Student-Id header is required." });

        var result = await _service.CreateHoldAsync(classId, studentId);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    private string? GetStudentId()
    {
        var studentId = Request.Headers["X-Student-Id"].ToString();
        return string.IsNullOrWhiteSpace(studentId) ? null : studentId;
    }
}
