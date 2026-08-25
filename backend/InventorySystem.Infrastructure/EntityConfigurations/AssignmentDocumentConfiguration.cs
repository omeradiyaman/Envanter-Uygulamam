using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.EntityConfigurations;

public sealed class AssignmentDocumentConfiguration : IEntityTypeConfiguration<AssignmentDocument>
{
    public void Configure(EntityTypeBuilder<AssignmentDocument> builder)
    {
        builder.ToTable("AssignmentDocuments");
        builder.HasKey(document => document.Id);
        builder.Property(document => document.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(document => document.StoredFileName).HasMaxLength(100).IsRequired();
        builder.Property(document => document.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(document => document.Description).HasMaxLength(500);
        builder.HasIndex(document => document.StoredFileName).IsUnique();
        builder.HasIndex(document => new { document.PersonnelId, document.UploadedAt });
        builder.HasQueryFilter(document => !document.IsDeleted);

        builder.HasOne(document => document.Personnel)
            .WithMany()
            .HasForeignKey(document => document.PersonnelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(document => document.AssignmentHistory)
            .WithMany()
            .HasForeignKey(document => document.AssignmentHistoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(document => document.ReplacesDocument)
            .WithMany()
            .HasForeignKey(document => document.ReplacesDocumentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
