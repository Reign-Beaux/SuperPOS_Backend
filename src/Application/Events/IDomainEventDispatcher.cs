using Domain.Events;

namespace Application.Events;

/// <summary>
/// Interface for dispatching domain events to their handlers.
/// Implementations should handle the dispatch mechanism (in-memory, async, etc.).
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches a domain event to all registered handlers.
    /// </summary>
    /// <param name="domainEvent">The event to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches multiple domain events to their handlers.
    /// </summary>
    /// <param name="domainEvents">The events to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DispatchManyAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
