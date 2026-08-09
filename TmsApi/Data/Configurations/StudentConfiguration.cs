using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        // Table name (optional - defaults to class name)
        builder.ToTable("Students");

        // Primary key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.Name)
            .IsRequired()           // NOT NULL
            .HasMaxLength(100);     // Max 100 characters

        builder.Property(s => s.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.GPA)
            .HasPrecision(3, 2);    // 3 total digits, 2 decimal places (like 3.80)

        // Index for faster queries
        builder.HasIndex(s => s.RegistrationNumber)
            .IsUnique();            // No two students can have same registration number

        builder.HasIndex(s => s.Name);

        // Shadow property for audit — exists in DB but not in the C# entity class
        builder.Property<DateTime>("LastUpdated")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Concurrency token — EF Core checks this on every UPDATE
        builder.Property(s => s.Version)
            .IsRowVersion();

        // Soft delete filter — excluded deleted students from ALL queries automatically
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
