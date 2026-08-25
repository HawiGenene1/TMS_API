using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Application.Services;
using TmsApi.Api.Configuration;
using TmsApi.Api.Filters;
using TmsApi.Api.Middleware;
using TmsApi.Middleware;
using TmsApi.Api.Authentication;
using Asp.Versioning;
using Scalar.AspNetCore;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Behaviors;
using TmsApi.Api.Handlers;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Services: add authentication / authorization services
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();  // Global filter
});

// Tell ASP.NET about both API versions
builder.Services.AddApiVersioning(options =>
{
    // Default to V1 if client doesn't specify
    options.DefaultApiVersion = new ApiVersion(1, 0);
    // Accept URLs without version (like /api/courses)
    options.AssumeDefaultVersionWhenUnspecified = true;
    // Tell clients what versions exist (adds headers)
    options.ReportApiVersions = true;
    // Version goes in the URL: /api/v1/...
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    // How to display version in docs
    options.GroupNameFormat = "'v'VV";
    options.SubstituteApiVersionInUrl = true;
})
.AddOpenApi();

builder.Services.AddSingleton<EnrollmentWorker>();

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<ICourseService, CourseService>();

// Register MediatR and all handlers
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly));

// Register all validators
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);

// Register pipeline behaviors (ORDER MATTERS!)
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));   // Logging FIRST
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)); // Validation SECOND

// Register global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

app.UseExceptionHandler();  // Must come BEFORE MapControllers

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map OpenAPI endpoints with versioning
app.MapOpenApi().WithDocumentPerVersion();

app.UseMiddleware<V1DeprecationMiddleware>();

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

// Minimal public scalar endpoint
app.MapGet("/scalar/v1", () => Results.Ok(new { value = 42 })).AllowAnonymous();

// Configure Scalar API documentation UI
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("TMS API Reference")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        
        // Add both V1 and V2 to the dropdown
        options.AddDocument("v1", "API Version 1.0")
               .AddDocument("v2", "API Version 2.0");
    });
}

// Seed data in development only
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}

app.Run();
