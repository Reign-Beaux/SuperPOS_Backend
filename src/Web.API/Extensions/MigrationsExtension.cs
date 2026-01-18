using Infrastructure.Persistence.Context;

namespace Web.API.Extensions;

internal static class MigrationsExtension
{
  public static void ApplyMigrations(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SuperPOSDbContext>();
    dbContext.Database.Migrate();
  }
}

