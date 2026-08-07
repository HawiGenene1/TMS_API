using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Service;
using TmsApi.Configuration;
using TmsApi.Data;
using TmsApi.Entities;

var builder = WebApplication.CreateBuilder(args);

// Services: add authentication / authorization services
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddSingleton<EnrollmentWorker>();

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();
    


builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddProblemDetails();

// Register TmsDbContext scoped for incoming HTTP requests
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
           .LogTo(Console.WriteLine, LogLevel.Information)  // Show SQL!
           .EnableSensitiveDataLogging());                  // Show parameter values

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Running in Development mode");
}
else
{
    Console.WriteLine("Running in Production mode");
}

// Middleware pipeline — order matters
app.UseMiddleware<RequestLoggingMiddleware>(); // outermost: stamps correlation id and logs every request

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Protected endpoint — anonymous callers get 401
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

// Seed test data at startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    context.Database.Migrate();  // Apply any pending migrations

    if (!context.Students.Any())
    {
        // Create 5 students
        var students = new List<Student>
        {
            new() { RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith",    GPA = 3.8m, IsActive = true  },
            new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones",      GPA = 2.9m, IsActive = true  },
            new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown",  GPA = 3.4m, IsActive = false },
            new() { RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince",   GPA = 3.9m, IsActive = true  },
            new() { RegistrationNumber = "TMS-2026-0005", Name = "Evan Wright",    GPA = 2.5m, IsActive = true  }
        };
        context.Students.AddRange(students);

        // Create 3 courses
        var courses = new List<Course>
        {
            new() { Code = "CS-101",  Title = "Introduction to Computer Science", Capacity = 30 },
            new() { Code = "CS-201",  Title = "Data Structures and Algorithms",   Capacity = 25 },
            new() { Code = "MAT-101", Title = "Calculus I",                       Capacity = 40 }
        };
        context.Courses.AddRange(courses);
        context.SaveChanges();

        // Create enrollments
        var enrollments = new List<Enrollment>
        {
            new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
            new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
            new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
            new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
        };
        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }
}

app.Run();
