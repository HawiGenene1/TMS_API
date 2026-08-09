using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(TmsDbContext context) : ControllerBase
{
    // GET: api/students?page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // 1. Always OrderBy first! (Stable sorting)
        var query = context.Students.OrderBy(s => s.Name);  // ✅ Sort alphabetically

        // 2. Calculate how many to skip
        int skip = (page - 1) * pageSize;

        // 3. Apply pagination
        var students = await query
            .Skip(skip)      // Skip previous pages
            .Take(pageSize)  // Take only this page
            .ToListAsync();

        // 4. Get total count for pagination info
        var totalCount = await context.Students.CountAsync();

        return Ok(new
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Students = students
        });
    }
}
