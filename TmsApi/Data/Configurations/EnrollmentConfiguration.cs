using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        builder.HasKey(e => e.Id);

        // Foreign Key configurations
        builder.Property(e => e.StudentId)
            .IsRequired();

        builder.Property(e => e.CourseId)
            .IsRequired();

        builder.Property(e => e.Grade)
            .HasPrecision(3, 2);  // Like 4.00, 3.50

        builder.Property(e => e.Year)
            .IsRequired()
            .HasDefaultValue(DateTime.UtcNow.Year);

        builder.Property(e => e.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        // Global query filter: archived enrollments are hidden from all queries
        builder.HasQueryFilter(e => !e.IsArchived);

        builder.Property(e => e.EnrolledAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Unique constraint: one student can enroll in same course only once
        builder.HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();

        // If a student is deleted, remove their enrollments (Cascade)
        // If a course has enrollments, prevent deletion (Restrict)
        // This protects student records from being orphaned if a course is accidentally deleted
        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);   // If student is deleted, delete their enrollments

        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);  // ⚠️ Can't delete course with enrollments!
    }
}
