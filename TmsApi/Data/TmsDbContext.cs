using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Data;

public class TmsDbContext(DbContextOptions<TmsDbContext> options) 
    : DbContext(options)
{
    // These are your database tables!
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
}