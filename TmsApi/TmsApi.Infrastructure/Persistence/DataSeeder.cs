using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;

public static class DataSeeder
{
    private static readonly (string Code, string Title, int MaxCapacity)[] Courses =
    [
        ("CSE-101", "Web Development Fundamentals", 30),
        ("CSE-102", "TypeScript Essentials", 30),
        ("CSE-103", "Git and Collaborative Workflows", 25),
        ("CSE-201", "ASP.NET Core Fundamentals", 28),
        ("CSE-202", "Entity Framework Core and PostgreSQL", 28),
        ("CSE-203", "Building RESTful Web APIs", 28),
        ("CSE-301", "Advanced Web API Patterns", 24),
        ("CSE-302", "Angular Fundamentals", 26),
        ("CSE-303", "Angular Advanced", 24),
        ("CSE-304", "Full-Stack Integration", 22),
        ("CSE-305", "Testing and Quality Assurance", 22),
        ("CSE-306", "Security and Authentication", 20),
        ("CSE-307", "Cloud Deployment", 20),
        ("CSE-401", "Software Architecture", 24),
        ("CSE-402", "Database Design", 25),
        ("CSE-403", "DevOps Fundamentals", 22),
        ("CSE-404", "Machine Learning Introduction", 20),
        ("CSE-405", "Data Science", 18),
        ("CSE-406", "Project Management", 24),
        ("CSE-407", "Business Intelligence", 20),
        ("CSE-408", "Digital Transformation", 22),
        ("CSE-409", "Software Quality Assurance", 20),
        ("CSE-410", "Web Security", 18),
        ("UX-101", "User Experience Fundamentals", 25),
        ("UX-201", "UX Research Methods", 20)
    ];

    public static async Task SeedAsync(TmsDbContext context, CancellationToken ct = default)
    {
        await context.Database.MigrateAsync(ct);

        if (await context.Courses.AnyAsync(ct))
            return;

        foreach (var (code, title, maxCapacity) in Courses)
        {
            context.Courses.Add(new Course
            {
                Code = code,
                Title = title,
                MaxCapacity = maxCapacity
            });
        }

        await context.SaveChangesAsync(ct);
    }
}
