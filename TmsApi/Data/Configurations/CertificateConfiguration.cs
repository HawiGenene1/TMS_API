using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.SerialNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.IssuedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
    }
}
