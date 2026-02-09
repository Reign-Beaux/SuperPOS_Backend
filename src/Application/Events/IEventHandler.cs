using Domain.Events;

namespace Application.Events;

/// <summary>
/// Handler interface for domain events.
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task Handle(TEvent domainEvent, CancellationToken cancellationToken = default);
}
