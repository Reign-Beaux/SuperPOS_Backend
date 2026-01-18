using Application.DesignPatterns.Mediators.Interfaces;

namespace Application.DesignPatterns.Mediators;

public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(request.GetType(), typeof(TResponse));

        dynamic? handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"Handler not found for {request.GetType().Name}");

        // Obtener todos los pipelines registrados
        var pipelineType = typeof(IPipelineBehavior<,>)
            .MakeGenericType(request.GetType(), typeof(TResponse));

        var pipelines = _serviceProvider
            .GetServices(pipelineType)
            .Cast<dynamic>()
            .Reverse()
            .ToList();

        // Encadenar los pipelines
        Func<Task<TResponse>> handlerDelegate = () => handler.Handle((dynamic)request, cancellationToken);

        foreach (var pipeline in pipelines)
        {
            var next = handlerDelegate;
            handlerDelegate = () => pipeline.Handle((dynamic)request, cancellationToken, next);
        }

        return await handlerDelegate();
    }

    public async Task Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        var handlerType = typeof(INotificationHandler<>)
            .MakeGenericType(notification.GetType());

        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (dynamic? handler in handlers)
        {
            if (handler is not null)
            {
                await handler.Handle((dynamic)notification, cancellationToken);
            }
        }
    }
}
