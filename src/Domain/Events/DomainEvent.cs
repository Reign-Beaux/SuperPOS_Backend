namespace Domain.Events;

/// <summary>
/// Base class for all domain events.
/// Provides default implementations for EventId and OccurredOn.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    public Guid EventId { get; } = Guid.CreateVersion7();

    /// <summary>
    /// Timestamp when this event occurred (UTC).
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    protected DomainEvent()
    {
    }
}
