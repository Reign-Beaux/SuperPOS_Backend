namespace Domain.Entities;

/// <summary>
/// Base class for all entities in the domain.
/// Provides common properties for identity, auditing, and soft delete.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for this entity (Guid v7 for time-ordered IDs).
    /// Protected internal setter allows EF Core and infrastructure access while preventing external modification.
    /// </summary>
    public Guid Id { get; protected internal set; } = Guid.CreateVersion7();

    /// <summary>
    /// Timestamp when this entity was created.
    /// Protected internal setter allows EF Core and infrastructure access for auditing.
    /// </summary>
    public DateTime CreatedAt { get; protected internal set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when this entity was last updated.
    /// Protected internal setter allows infrastructure layer to track modifications.
    /// </summary>
    public DateTime? UpdatedAt { get; protected internal set; }

    /// <summary>
    /// Timestamp when this entity was soft-deleted.
    /// Protected internal setter allows infrastructure layer to perform soft deletes.
    /// </summary>
    public DateTime? DeletedAt { get; protected internal set; }

    /// <summary>
    /// Performs a soft delete by setting the DeletedAt timestamp.
    /// This is the proper way to delete an entity - preserves data for audit trails.
    /// </summary>
    public virtual void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if this entity has been soft-deleted.
    /// </summary>
    public bool IsDeleted() => DeletedAt.HasValue;

    /// <summary>
    /// Marks this entity as updated by setting the UpdatedAt timestamp.
    /// Derived classes should call this when making modifications.
    /// </summary>
    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
