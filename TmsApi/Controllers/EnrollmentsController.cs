using Microsoft.AspNetCore.Mvc;
using TmsApi.Service;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    // GET: /api/enrollments
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await _enrollmentService.GetAllAsync();
        return Ok(enrollments);
    }

    // GET: /api/enrollments/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await _enrollmentService.GetByIdAsync(id);

     if (record == null)
{
    return NotFound(new ProblemDetails
    {
        Title = "Enrollment not found",
        Detail = $"No enrollment exists with id '{id}'.",
        Status = StatusCodes.Status404NotFound
    });
}
        return Ok(record);
    }

    // POST: /api/enrollments
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
{
    var record = await _enrollmentService.EnrollAsync(
        request.StudentId,
        request.CourseCode);

    return CreatedAtAction(
        nameof(GetById),
        new { id = record.Id },
        record);
}

// DELETE: /api/enrollments/{id}
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(string id)
{
    var deleted = await _enrollmentService.DeleteAsync(id);

   if (!deleted)
{
    return NotFound(new ProblemDetails
    {
        Title = "Enrollment not found",
        Detail = $"No enrollment exists with id '{id}'.",
        Status = StatusCodes.Status404NotFound
    });
}
    return NoContent();
}
}


public record CreateEnrollmentRequest(
    string StudentId,
    string CourseCode
);