using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
using System.Linq;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context, IServiceProvider services) : ControllerBase
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

    // N+1 Problem Demonstration
    [HttpGet("n-plus-one")]
    public async Task<IActionResult> TestNPlusOne()
    {
        Console.WriteLine("\n🚨 RUNNING N+1 DEMONSTRATION...");
        Console.WriteLine("This will show 1 + N queries in the log!");

        // Load all students (1 query)
        var students = await context.Students.AsNoTracking().ToListAsync();

        Console.WriteLine($"\n✅ Loaded {students.Count} students");
        Console.WriteLine("Now counting enrollments for each student...");

        var results = new List<object>();

        // For EACH student, query the database again (N queries)
        foreach (var student in students)
        {
            var count = await context.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.StudentId == student.Id);

            results.Add(new
            {
                StudentName = student.Name,
                EnrollmentCount = count
            });

            Console.WriteLine($"   {student.Name}: {count} enrollments");
        }

        Console.WriteLine($"\n✅ Total queries: {1 + students.Count}");
        Console.WriteLine("Check the SQL log above!");
        Console.WriteLine("This is the N+1 problem in action!\n");

        return Ok(results);
    }

    // N+1 Fix: Single query with projection
    [HttpGet("n-plus-one-fixed")]
    public async Task<IActionResult> TestNPlusOneFixed()
    {
        Console.WriteLine("\n✅ RUNNING FIXED VERSION (Single Query)...");

        // One query with projection
        var results = await context.Students
            .AsNoTracking()
            .Select(s => new
            {
                StudentName = s.Name,
                EnrollmentCount = s.Enrollments.Count  // EF translates to SQL subquery!
            })
            .ToListAsync();

        foreach (var r in results)
        {
            Console.WriteLine($"   {r.StudentName}: {r.EnrollmentCount} enrollments");
        }

        Console.WriteLine("\n✅ Only 1 SQL query was executed!");
        Console.WriteLine("Check the SQL log - it uses a subquery!");

        return Ok(results);
    }

    // N+1 Fix: Use Include (LEFT JOIN)
    [HttpGet("n-plus-one-include")]
    public async Task<IActionResult> TestNPlusOneInclude()
    {
        Console.WriteLine("\n✅ USING INCLUDE (Single Query)...");

        // One query with Include loads all related data
        var students = await context.Students
            .AsNoTracking()
            .Include(s => s.Enrollments)  // Load all enrollments in one query
            .ToListAsync();

        foreach (var student in students)
        {
            Console.WriteLine($"   {student.Name}: {student.Enrollments.Count} enrollments");
        }

        Console.WriteLine("\n✅ Only 1 SQL query was executed!");
        Console.WriteLine("Check the SQL log - it uses a LEFT JOIN!");

        return Ok(students);
    }

    // Concurrency Token Test
    [HttpGet("concurrency-test")]
    public async Task<IActionResult> TestConcurrency()
    {
        try
        {
            // Get first student
            var student = await context.Students.FindAsync(1);
            if (student is null) return NotFound("Student with Id=1 not found.");

            Console.WriteLine($"Original Name: {student.Name}, GPA: {student.GPA}");

            // Update 1: Change name
            student.Name = "Updated Name";
            await context.SaveChangesAsync();
            Console.WriteLine("✅ First update successful");

            // Update 2: Change GPA (same context, same version — succeeds)
            student.GPA = 4.0m;
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Second update successful");

            // Simulate conflict: get a fresh context with a new copy of the student
            using var freshContext = services.GetRequiredService<TmsDbContext>();
            var freshStudent = await freshContext.Students.FindAsync(1);
            freshStudent!.Name = "Fresh Update";
            await freshContext.SaveChangesAsync();
            Console.WriteLine("✅ Fresh context update successful (version now incremented)");

            // Now try to save with the stale original context — version mismatch!
            student.Name = "Outdated Update";
            await context.SaveChangesAsync();  // ❌ THROWS DbUpdateConcurrencyException!

            return Ok("Test passed!");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return BadRequest(new
            {
                Message = "Concurrency conflict detected! ✅ This is expected.",
                Error = ex.Message
            });
        }
    }
}