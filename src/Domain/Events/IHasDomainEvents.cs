namespace Domain.Events;

/// <summary>
/// Interface for entities that can raise domain events.
/// Aggregate roots should implement this interface to collect events
/// that will be dispatched after successful persistence.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events from this entity.
    /// Called after events have been dispatched.
    /// </summary>
    void ClearDomainEvents();
}
