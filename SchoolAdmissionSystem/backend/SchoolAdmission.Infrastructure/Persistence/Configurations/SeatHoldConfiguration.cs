using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Infrastructure.Persistence.Configurations;

public class SeatHoldConfiguration
    : IEntityTypeConfiguration<SeatHold>
{
    public void Configure(
        EntityTypeBuilder<SeatHold> builder)
    {
        builder.ToTable("seat_holds", table =>
        {
            table.HasCheckConstraint(
                "CK_seat_holds_student_id",
                "length(trim(\"StudentId\")) > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ClassId)
            .IsRequired();

        builder.Property(x => x.StudentId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.ExpiresAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.ConfirmedAt)
            .HasColumnType("timestamp with time zone");

        // A class must exist before a student can hold its seat.
        builder.HasOne<AdmissionClass>()
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // One active or confirmed application per student per class.
        builder.HasIndex(x => new { x.ClassId, x.StudentId })
            .IsUnique()
            .HasFilter("\"Status\" IN (1, 2)");

        // Helps calculate held and confirmed seats for a class.
        builder.HasIndex(x => new { x.ClassId, x.Status });

        // Helps retrieve a student's applications.
        builder.HasIndex(x => new { x.StudentId, x.ClassId });

        // Helps find holds that have reached their expiry time.
        builder.HasIndex(x => new { x.Status, x.ExpiresAt });
    }
}