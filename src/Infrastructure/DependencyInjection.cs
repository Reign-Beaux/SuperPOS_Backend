using Application.Interfaces.Persistence.Context;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.Interfaces.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Context;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services
            .AddCaching()
            .AddPersistence(configuration)
            .AddServices();

        return services;
    }

    private static IServiceCollection AddCaching(this IServiceCollection services)
    {
        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services;
    }
}
