using Microsoft.Extensions.DependencyInjection;
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
            .AddFeaturesServices();

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
}
