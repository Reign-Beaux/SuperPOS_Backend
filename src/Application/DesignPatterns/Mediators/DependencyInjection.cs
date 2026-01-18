using System.Reflection;
using Application.DesignPatterns.Mediators.Interfaces;

namespace Application.DesignPatterns.Mediators;

public static class DependencyInjection
{
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Register the central Mediator
        services.AddScoped<IMediator, Mediator>();

        if (assemblies == null || assemblies.Length == 0)
        {
            return services;
        }

        // Flatten all types from all provided assemblies
        var types = assemblies.SelectMany(a => a.GetTypes()).ToList();

        // Register all handlers (commands, queries, notifications)
        var handlerTypes = types
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                 i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))))
            .ToList();

        foreach (var handler in handlerTypes)
        {
            var interfaces = handler.GetInterfaces().Where(i =>
                i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                 i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)));

            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, handler);
            }
        }

        return services;
    }
}
