using Application.Events;
using Domain.Events;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Application;

/// <summary>
/// Domain event dispatcher that resolves and executes registered event handlers.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<DomainEventDispatcher> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(
        ILogger<DomainEventDispatcher> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Dispatches a single domain event to all registered handlers.
    /// </summary>
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType();

        _logger.LogInformation(
            "Domain Event Dispatched: {EventType} (EventId: {EventId}, OccurredOn: {OccurredOn})",
            eventType.Name,
            domainEvent.EventId,
            domainEvent.OccurredOn);

        // Find all registered handlers for this event type
        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handlers = _serviceProvider.GetService(handlerType);

        if (handlers != null)
        {
            var handleMethod = handlerType.GetMethod("Handle");
            if (handleMethod != null)
            {
                try
                {
                    await (Task)handleMethod.Invoke(handlers, new object[] { domainEvent, cancellationToken })!;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling domain event {EventType}", eventType.Name);
                }
            }
        }
        else
        {
            _logger.LogWarning("No handler found for event type {EventType}", eventType.Name);
        }
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
