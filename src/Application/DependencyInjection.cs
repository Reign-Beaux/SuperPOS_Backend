using Application.DesignPatterns.Mediators;
using Application.Events;
using Application.EventHandlers;
using Domain.Events.Inventories;
using Domain.Events.Returns;
using Domain.Events.Sales;
using Mapster;
using MapsterMapper;
using System.Reflection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddValidatorsFromAssembly(assembly);

        services
            .AddBehaviors()
            .AddFeaturesServices()
            .AddEventHandlers()
            .AddMediator(assembly);

        services.AddMapster();

        return services;
    }

    private static IServiceCollection AddMapster(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton(config);
        services.AddScoped<IMapper, Mapper>();

        return services;
    }

    public static IServiceCollection AddBehaviors(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddFeaturesServices(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        // Register event handlers
        services.AddScoped<IEventHandler<LowStockEvent>, LowStockEventHandler>();
        services.AddScoped<IEventHandler<SaleCancelledEvent>, SaleCancelledEventHandler>();
        services.AddScoped<IEventHandler<ReturnApprovedEvent>, ReturnApprovedEventHandler>();

        return services;
    }
}
