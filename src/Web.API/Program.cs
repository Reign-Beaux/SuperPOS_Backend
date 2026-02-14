using Application;
using Infrastructure;
using Web.API;
using Web.API.Extensions;
using Web.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder
    .Configuration
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var cors = "Cors";
var origins = builder.Configuration.GetSection("CorsSettings:Origins").Get<string[]>();

if (origins is null || origins.Length == 0)
    throw new InvalidOperationException("No se configuraron los orígenes permitidos para CORS (CorsSettings:AllowedOrigins).");

// Add services to the container.
builder.Services
    .AddWebAPI(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy(
      name: cors,
      builder =>
      {
          builder
              .WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.ApplyMigrations();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();
app.UseCors(cors);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
