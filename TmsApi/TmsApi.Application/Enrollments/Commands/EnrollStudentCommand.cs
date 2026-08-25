using MediatR;
using TmsApi.Application.Common;

namespace TmsApi.Application.Enrollments.Commands;

// This is what the user wants to do
public record EnrollStudentCommand(int StudentId, string CourseCode) 
    : IRequest<Result<EnrollmentCreated, EnrollmentError>>;

// What we return on success
public record EnrollmentCreated(int EnrollmentId, int StudentId, string CourseCode);
