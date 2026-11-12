using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Infrastructure.Persistence.Configurations;

public class AdmissionClassConfiguration
    : IEntityTypeConfiguration<AdmissionClass>
{
    public void Configure(
        EntityTypeBuilder<AdmissionClass> builder)
    {
        builder.ToTable("admission_classes", table =>
        {
            table.HasCheckConstraint(
                "CK_admission_classes_total_seats",
                "\"TotalSeats\" > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TotalSeats)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => x.Name);
    }
}