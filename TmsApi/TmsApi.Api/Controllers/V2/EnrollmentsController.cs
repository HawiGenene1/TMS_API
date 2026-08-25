using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;

namespace TmsApi.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Enroll(
        EnrollStudentCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            // Success → 201 Created with Location header
            onSuccess: created => CreatedAtAction(
                nameof(GetSchedule),
                new { studentId = created.StudentId },
                created),
            
            // Failure → Map error to appropriate status code
            onFailure: error => error.Code switch
            {
                "course_not_found" => NotFound(new ProblemDetails
                {
                    Title = "Course Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = error.Message,
                    Type = "https://tms.local/errors/course_not_found"
                }),
                
                "course_full" or "already_enrolled" => Conflict(new ProblemDetails
                {
                    Title = "Enrollment Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = error.Message,
                    Type = "https://tms.local/errors/" + error.Code
                }),
                
                _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Unknown Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred.",
                    Type = "https://tms.local/errors/unknown"
                })
            });
    }

    [HttpGet("{studentId}/schedule")]
    public async Task<IActionResult> GetSchedule(
        int studentId,
        CancellationToken ct)
    {
        var schedule = await mediator.Send(new GetStudentScheduleQuery(studentId), ct);
        return Ok(schedule);
    }
}
