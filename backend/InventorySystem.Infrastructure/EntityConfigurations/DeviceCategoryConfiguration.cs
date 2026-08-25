using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.EntityConfigurations;

public sealed class DeviceCategoryConfiguration : IEntityTypeConfiguration<DeviceCategory>
{
    public void Configure(EntityTypeBuilder<DeviceCategory> builder)
    {
        builder.ToTable("DeviceCategories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(category => category.Description)
            .HasMaxLength(500);

        builder.HasIndex(category => category.Name)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasQueryFilter(category => !category.IsDeleted);

        // Seed data (using deterministic GUIDs)
        var now = new DateTimeOffset(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);
        builder.HasData(
            CreateSeedCategory(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Laptop", now),
            CreateSeedCategory(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Monitör", now),
            CreateSeedCategory(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Masaüstü Bilgisayar", now),
            CreateSeedCategory(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Yazıcı", now),
            CreateSeedCategory(Guid.Parse("55555555-5555-5555-5555-555555555555"), "Tablet", now),
            CreateSeedCategory(Guid.Parse("66666666-6666-6666-6666-666666666666"), "Telefon", now),
            CreateSeedCategory(Guid.Parse("77777777-7777-7777-7777-777777777777"), "Zbox", now)
        );
    }

    private static DeviceCategory CreateSeedCategory(Guid id, string name, DateTimeOffset date)
    {
        var category = new DeviceCategory(name, null);
        
        typeof(InventorySystem.Domain.Common.BaseEntity)
            .GetProperty("Id")!
            .SetValue(category, id);
        
        typeof(InventorySystem.Domain.Common.BaseEntity)
            .GetProperty("CreatedAt")!
            .SetValue(category, date);
            
        return category;
    }
}
