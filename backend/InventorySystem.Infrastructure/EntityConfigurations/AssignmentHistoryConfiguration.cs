using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.EntityConfigurations;

public sealed class AssignmentHistoryConfiguration : IEntityTypeConfiguration<AssignmentHistory>
{
    public void Configure(EntityTypeBuilder<AssignmentHistory> builder)
    {
        builder.ToTable("AssignmentHistories");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Note)
            .HasMaxLength(500);

        builder.HasIndex(a => new { a.DeviceId, a.ReturnedAt })
            .HasFilter("\"ReturnedAt\" IS NULL")
            .IsUnique()
            .HasDatabaseName("IX_AssignmentHistories_DeviceId_ActiveAssignment");

        // Device relationship — Restrict to protect history
        builder.HasOne(a => a.Device)
            .WithMany(d => d.AssignmentHistory)
            .HasForeignKey(a => a.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Personnel relationship — Restrict to protect history
        builder.HasOne(a => a.Personnel)
            .WithMany()
            .HasForeignKey(a => a.PersonnelId)
            .OnDelete(DeleteBehavior.Restrict);

        // No soft-delete filter — history records should always be visible
    }
}
