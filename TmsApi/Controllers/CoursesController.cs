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
}
