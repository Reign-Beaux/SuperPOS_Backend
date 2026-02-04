namespace Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something important that happened in the domain.
/// They are used for cross-aggregate communication and triggering side effects.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp when this event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
