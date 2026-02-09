using Application.Events;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Repositories;
using Domain.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services.Application;
using Infrastructure.Services.Domain;

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

        // Register Unit of Work (provides access to all repositories)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register specific repositories (optional - they're available through UnitOfWork)
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        // Register generic repository base for minor entities
        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IEncryptionService, EncryptionService>();

        // Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Domain services
        services.AddScoped<IProductUniquenessChecker, ProductUniquenessChecker>();
        services.AddScoped<ICustomerUniquenessChecker, CustomerUniquenessChecker>();
        services.AddScoped<IUserUniquenessChecker, UserUniquenessChecker>();
        services.AddScoped<ISaleValidationService, SaleValidationService>();
        services.AddScoped<IStockReservationService, StockReservationService>();

        return services;
    }
}
