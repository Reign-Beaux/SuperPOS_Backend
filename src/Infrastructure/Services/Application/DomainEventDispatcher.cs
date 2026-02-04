using Application.Events;
using Domain.Events;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Application;

/// <summary>
/// Simple domain event dispatcher implementation.
/// Currently logs events for demonstration.
/// In a production system, this would dispatch to registered event handlers.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(ILogger<DomainEventDispatcher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Dispatches a single domain event.
    /// </summary>
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Log the event for demonstration
        _logger.LogInformation(
            "Domain Event Dispatched: {EventType} (EventId: {EventId}, OccurredOn: {OccurredOn})",
            domainEvent.GetType().Name,
            domainEvent.EventId,
            domainEvent.OccurredOn);

        // In a production system, you would:
        // 1. Find all registered handlers for this event type
        // 2. Invoke each handler asynchronously
        // 3. Handle errors and retries
        // 4. Optionally publish to message broker for external systems

        await Task.CompletedTask;
    }

    /// <summary>
    /// Dispatches multiple domain events.
    /// </summary>
    public async Task DispatchManyAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
