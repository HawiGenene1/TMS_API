using TmsApi.Application.DTOs;

namespace TmsApi.Application.Services;

public interface ICourseService
{
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PageRequest request, CancellationToken ct);
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
}
