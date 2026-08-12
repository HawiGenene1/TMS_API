using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.GPA)
            .HasPrecision(3, 2);

        builder.HasIndex(s => s.RegistrationNumber)
            .IsUnique();

        builder.HasIndex(s => s.Name);

        // Shadow property for audit — exists in DB but not in the C# entity class
        builder.Property<DateTime>("LastUpdated")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Concurrency token — EF Core checks this on every UPDATE
        builder.Property(s => s.Version)
            .IsRowVersion();

        // Soft delete global filter
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
