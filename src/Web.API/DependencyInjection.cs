using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Web.API.Middlewares;

namespace Web.API;

public static class DependencyInjection
{
    public static IServiceCollection AddWebAPI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddSerializations();
        services.AddJwtConfiguration(configuration);
        services.AddScoped<GlobalExceptionHandlingMiddleware>();
        services.AddScoped<SecurityHeadersMiddleware>();

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
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured. Configure it using User Secrets or environment variables.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Set to true in production
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(int.Parse(jwtSettings["ClockSkewMinutes"] ?? "5"))
                };
            });

        services.AddAuthorization(options =>
        {
            // Admin tiene acceso total
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Administrador"));

            // Gestión: Solo Gerente y Admin (NO Vendedor)
            options.AddPolicy("ManagementOnly", policy =>
                policy.RequireRole("Administrador", "Gerente"));

            // POS: Solo Vendedor y Admin (NO Gerente)
            options.AddPolicy("POSOnly", policy =>
                policy.RequireRole("Administrador", "Vendedor"));
        });

        return services;
    }
}
