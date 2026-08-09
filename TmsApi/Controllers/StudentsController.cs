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

    // DELETE: api/students/1  (soft delete — marks as deleted, doesn't remove from DB)
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteStudent(int id)
    {
        var student = await context.Students.FindAsync(id);
        if (student == null)
            return NotFound();

        student.IsDeleted = true;
        await context.SaveChangesAsync();

        return Ok(new { Message = $"Student {student.Name} has been soft-deleted" });
    }

    // GET: api/students/admin/deleted  (admin only — see all soft-deleted students)
    [HttpGet("admin/deleted")]
    public async Task<IActionResult> GetDeletedStudents()
    {
        var deleted = await context.Students
            .IgnoreQueryFilters()       // Bypass the soft-delete filter
            .Where(s => s.IsDeleted)
            .ToListAsync();

        return Ok(deleted);
    }

    // POST: api/students/admin/restore/1  (admin only — restore a soft-deleted student)
    [HttpPost("admin/restore/{id}")]
    public async Task<IActionResult> RestoreStudent(int id)
    {
        var student = await context.Students
            .IgnoreQueryFilters()       // Need this to find deleted students
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return NotFound();

        student.IsDeleted = false;
        await context.SaveChangesAsync();

        return Ok(new { Message = $"Student {student.Name} has been restored" });
    }
}
