using Web.API.Middlewares;

namespace Web.API;

public static class DependencyInjection
{
    public static IServiceCollection AddWebAPI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddSerializations();
        services.AddScoped<GlobalExceptionHandlingMiddleware>();
        
        return services;
    }

    private static IServiceCollection AddSerializations(this IServiceCollection services)
    {
        services.Configure<JsonOptions>(options =>
        {
            // options.JsonSerializerOptions.Converters.Add(new ValueObjectJsonConverterString<Rfc>());
        });

        return services;
    }

    private static IServiceCollection AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
    {

        return services;
    }
}
