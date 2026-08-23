using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.EntityConfigurations;

public sealed class PersonnelConfiguration : IEntityTypeConfiguration<Personnel>
{
    public void Configure(EntityTypeBuilder<Personnel> builder)
    {
        builder.ToTable("Personnel");

        builder.HasKey(personnel => personnel.Id);

        builder.Property(personnel => personnel.SicilNo)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(personnel => personnel.Ad)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(personnel => personnel.Soyad)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(personnel => personnel.Departman)
            .HasMaxLength(120)
            .IsRequired();
        builder.Property(personnel => personnel.Pozisyon)
            .HasMaxLength(120)
            .IsRequired();
        builder.Property(personnel => personnel.ZimmetNo)
            .HasMaxLength(50);

        builder.HasIndex(personnel => personnel.SicilNo)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(personnel => personnel.ZimmetNo);

        builder.HasQueryFilter(personnel => !personnel.IsDeleted);
    }
}
