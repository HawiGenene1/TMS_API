using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Data;
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Services;

public class CourseService : ICourseService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<CourseService> _logger;

    public CourseService(TmsDbContext context, ILogger<CourseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PageRequest request, CancellationToken ct)
    {
        // STEP 1: Start with query (no tracking for reads)
        IQueryable<Course> query = _context.Courses.AsNoTracking();

        // STEP 2: Apply search filter (if provided)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, $"%{request.Search}%") ||
                EF.Functions.ILike(c.Code, $"%{request.Search}%"));
        }

        // STEP 3: Count TOTAL before pagination!
        // Runs: SELECT COUNT(*) FROM "Courses" WHERE ...
        var totalCount = await query.CountAsync(ct);

        // STEP 4: Apply sorting
        query = request.OrderBy.ToLower() switch
        {
            "code"        => request.Descending ? query.OrderByDescending(c => c.Code)        : query.OrderBy(c => c.Code),
            "maxcapacity" => request.Descending ? query.OrderByDescending(c => c.MaxCapacity) : query.OrderBy(c => c.MaxCapacity),
            // Default: order by Title
            _             => request.Descending ? query.OrderByDescending(c => c.Title)       : query.OrderBy(c => c.Title)
        };

        // STEP 5: Apply pagination and project
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .ToListAsync(ct);

        // STEP 6: Return paged response
        return new PagedResponse<CourseResponseDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = request.Page,
            PageSize   = request.PageSize
        };
    }

    public async Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Created course {CourseId} ({Code})",
            course.Id, course.Code);

        // Re-query through GetByIdAsync for consistent projection
        var result = await GetByIdAsync(course.Id, ct);
        return result!;  // Safe - we just created it
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct)
    {
        return await _context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Code == code, ct);
    }
}
