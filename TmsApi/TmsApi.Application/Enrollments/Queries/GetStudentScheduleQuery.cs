using MediatR;

namespace TmsApi.Application.Enrollments.Queries;

// Query to get a student's course schedule
public record GetStudentScheduleQuery(int StudentId) : IRequest<StudentScheduleDto>;

// Response DTO
public record StudentScheduleDto(
    int StudentId,
    IEnumerable<ScheduledCourseDto> Courses);

public record ScheduledCourseDto(
    int EnrollmentId,
    string CourseCode,
    string CourseTitle,
    DateTime EnrolledAt);
