using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using System.Linq;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>> STEP 1: Building query (no database contact)...");
        var query = context.Students.Where(s => s.GPA >= 3.0m);
        
        Console.WriteLine("\n>> STEP 2: Appending sorting clause...");
        var orderedQuery = query.OrderBy(s => s.Name);
        
        Console.WriteLine(">> STEP 3: Materializing query into C# List...");
        var results = orderedQuery.ToList();  // 🚀 Database query happens HERE!
        
        Console.WriteLine(">> STEP 4: Materialization finished.\n");
        return Ok(results);
    }
    
    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        // ✅ Inline expression — EF Core can translate this to SQL
        var students = context.Students
            .Where(s => s.GPA >= 3.5m)
            .ToList();
        return Ok(students);
    }

    // Query 1: Active Students with GPA >= 3.0
    [HttpGet("active-honor-students")]
    public async Task<IActionResult> GetActiveHonorStudents()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(new { ActiveStudentsWithGPA3OrHigher = count });
    }

    // Query 3: Average GPA per Course
    [HttpGet("course-average-gpa")]
    public async Task<IActionResult> GetCourseAverageGPA()
    {
        var averages = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(averages);
    }

    // Query 4A: Students with No Enrollments (Subquery)
    [HttpGet("no-enrollments-subquery")]
    public async Task<IActionResult> GetNoEnrollmentsSubquery()
    {
        var students = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(students);
    }

    // Query 4B: Students with No Enrollments (Left Join)
    [HttpGet("no-enrollments-leftjoin")]
    public async Task<IActionResult> GetNoEnrollmentsLeftJoin()
    {
        var students = await context.Students
            .LeftJoin(context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, e) => new { Student = s, Enrollment = e })
            .Where(x => x.Enrollment == null)
            .Select(x => x.Student.Name)
            .ToListAsync();

        return Ok(students);
    }

    // Query 2: Courses with Most Enrollments
    [HttpGet("popular-courses")]
    public async Task<IActionResult> GetPopularCourses()
    {
        var courses = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(courses);
    }
}