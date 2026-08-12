using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Data;

public class TmsDbContext(DbContextOptions<TmsDbContext> options) 
    : DbContext(options)
{
    // These are your database tables!
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ✅ Automatically picks up all IEntityTypeConfiguration classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TmsDbContext).Assembly);
    }

    // Helper: stamp the shadow audit property before saving
    public void UpdateAuditStamps(Student student)
    {
        Entry(student).Property("LastUpdated").CurrentValue = DateTime.UtcNow;
    }
}