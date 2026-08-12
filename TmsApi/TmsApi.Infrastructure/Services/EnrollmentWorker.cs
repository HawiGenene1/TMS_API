using Microsoft.Extensions.DependencyInjection;
using TmsApi.Application.Services;

namespace TmsApi.Infrastructure.Services;

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        using var scope = _scopeFactory.CreateScope();

        var enrollmentService =
            scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

        Console.WriteLine("Processing enrollments...");
    }
}
