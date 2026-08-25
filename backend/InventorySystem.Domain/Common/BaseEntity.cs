namespace InventorySystem.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; protected set; }

    public bool IsDeleted { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }
}
