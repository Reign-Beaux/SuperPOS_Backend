using Application.Interfaces.Persistence.UnitOfWorks;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Context;

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
        services.AddDbContext<SuperPOSDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("SuperPOS")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services;
    }
}
