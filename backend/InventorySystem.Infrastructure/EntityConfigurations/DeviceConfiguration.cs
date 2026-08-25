using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.EntityConfigurations;

public sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        builder.HasKey(device => device.Id);

        builder.Property(device => device.CihazAdi)
            .HasMaxLength(150)
            .IsRequired();
            
        builder.Property(device => device.SeriNo)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(device => device.EnvanterNo)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(device => device.BarkodNo)
            .HasMaxLength(100);
            
        builder.Property(device => device.Marka)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(device => device.Model)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(device => device.WarrantyStartDate).HasColumnType("date");
        builder.Property(device => device.WarrantyEndDate).HasColumnType("date");
        builder.Property(device => device.WarrantyProvider).HasMaxLength(200);
        builder.Property(device => device.WarrantyNote).HasMaxLength(1000);
        builder.HasIndex(device => device.WarrantyEndDate);

        // Unique indexes for active records
        builder.HasIndex(device => device.SeriNo)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
            
        builder.HasIndex(device => device.EnvanterNo)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
            
        builder.HasIndex(device => device.BarkodNo)
            .IsUnique()
            .HasFilter("\"BarkodNo\" IS NOT NULL AND \"IsDeleted\" = false");

        // Relationships
        builder.HasOne(device => device.Category)
            .WithMany()
            .HasForeignKey(device => device.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(device => device.Personel)
            .WithMany(personnel => personnel.Devices)
            .HasForeignKey(device => device.PersonelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(device => !device.IsDeleted);
    }
}
