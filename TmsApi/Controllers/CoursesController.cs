using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(TmsDbContext context) : ControllerBase
{
    // GET: api/courses/top-enrollments
    [HttpGet("top-enrollments")]
    public async Task<IActionResult> GetTopCourses()
    {
        var topCourses = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)  // Most popular first
            .Take(5)  // Only top 5
            .ToListAsync();

        return Ok(topCourses);
    }

    // POST: api/courses/archive-old
    // Bulk archive enrollments older than 2025 — one SQL UPDATE, no loading into memory
    [HttpPost("archive-old")]
    public async Task<IActionResult> ArchiveOldEnrollments()
    {
        var cutoffDate = new DateTime(2025, 12, 31);

        var count = await context.Enrollments
            .Where(e => e.EnrolledAt < cutoffDate && !e.IsArchived)
            .ExecuteUpdateAsync(e => e.SetProperty(x => x.IsArchived, true));

        return Ok(new
        {
            Message = $"Archived {count} enrollments",
            CutoffDate = cutoffDate,
            ArchivedCount = count
        });
    }
}
