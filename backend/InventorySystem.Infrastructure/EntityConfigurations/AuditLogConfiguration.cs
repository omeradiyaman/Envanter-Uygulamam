using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.EntityConfigurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(log => log.Id);
        builder.Property(log => log.ActionType).HasMaxLength(80).IsRequired();
        builder.Property(log => log.EntityType).HasMaxLength(80).IsRequired();
        builder.Property(log => log.Description).HasMaxLength(1000).IsRequired();
        builder.Property(log => log.OldValues).HasColumnType("jsonb");
        builder.Property(log => log.NewValues).HasColumnType("jsonb");
        builder.HasIndex(log => log.CreatedAt);
        builder.HasIndex(log => new { log.EntityType, log.EntityId });
        builder.HasIndex(log => log.OperationId);
        builder.HasIndex(log => log.UserId);
        builder.Ignore(log => log.IsDeleted);
        builder.Ignore(log => log.DeletedAt);
    }
}
