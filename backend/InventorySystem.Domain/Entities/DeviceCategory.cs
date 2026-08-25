using InventorySystem.Domain.Common;

namespace InventorySystem.Domain.Entities;

public sealed class DeviceCategory : BaseEntity
{
    private DeviceCategory()
    {
    }

    public DeviceCategory(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public void Update(string name, string? description, DateTimeOffset updatedAt)
    {
        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = updatedAt;
    }

    public void SoftDelete(DateTimeOffset deletedAt)
    {
        IsDeleted = true;
        DeletedAt = deletedAt;
        UpdatedAt = deletedAt;
    }
}
