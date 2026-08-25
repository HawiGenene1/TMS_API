using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Services;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Enrollments.Commands;

public class EnrollStudentHandler(
    IEnrollmentService enrollmentService,
    ICourseService courseService) 
    : IRequestHandler<EnrollStudentCommand, Result<EnrollmentCreated, EnrollmentError>>
{
    public async Task<Result<EnrollmentCreated, EnrollmentError>> Handle(
        EnrollStudentCommand command,
        CancellationToken ct)
    {
        // 1. Does the course exist?
        var course = await courseService.GetByCodeAsync(command.CourseCode, ct);
        if (course is null)
            return Result<EnrollmentCreated, EnrollmentError>.Failure(
                EnrollmentError.CourseNotFound(command.CourseCode));

        // 2. Is the course full?
        if (course.Enrollments.Count == course.MaxCapacity)
            return Result<EnrollmentCreated, EnrollmentError>.Failure(
                EnrollmentError.CourseFull(course.Title, course.MaxCapacity));

        // 3. Is the student already enrolled?
        if (await enrollmentService.ExistsAsync(command.StudentId, command.CourseCode, ct))
            return Result<EnrollmentCreated, EnrollmentError>.Failure(
                EnrollmentError.AlreadyEnrolled(command.StudentId, command.CourseCode));

        // 4. All good! Create the enrollment
        var enrollment = new Enrollment
        {
            StudentId = command.StudentId,
            CourseId = course.Id,
            EnrolledAt = DateTime.UtcNow
        };

        await enrollmentService.AddAsync(enrollment, ct);

        return Result<EnrollmentCreated, EnrollmentError>.Success(
            new EnrollmentCreated(
                enrollment.Id,
                command.StudentId,
                command.CourseCode));
    }
}
