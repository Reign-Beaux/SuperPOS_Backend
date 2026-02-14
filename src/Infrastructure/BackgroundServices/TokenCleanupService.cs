using Application.Interfaces.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundServices;

/// <summary>
/// Background service that automatically cleans up expired refresh tokens from the database.
/// Runs every 24 hours to keep the database clean and reduce storage overhead.
/// </summary>
public class TokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenCleanupService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(24); // Run every 24 hours
    private readonly TimeSpan _initialDelay = TimeSpan.FromMinutes(5); // First run after 5 minutes

    public TokenCleanupService(
        IServiceProvider serviceProvider,
        ILogger<TokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token Cleanup Service started. First run in {Delay} minutes, then every {Period} hours",
            _initialDelay.TotalMinutes,
            _period.TotalHours);

        // Wait for initial delay before first run
        await Task.Delay(_initialDelay, stoppingToken);

        using var timer = new PeriodicTimer(_period);

        // Run cleanup immediately after initial delay
        await DoWorkAsync(stoppingToken);

        // Then continue with periodic execution
        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            await DoWorkAsync(stoppingToken);
        }

        _logger.LogInformation("Token Cleanup Service stopped");
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting token cleanup...");

            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Delete tokens that expired more than 7 days ago
            var deletedCount = await unitOfWork.RefreshTokens.DeleteExpiredTokensAsync(cancellationToken);

            if (deletedCount > 0)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Token cleanup completed successfully. {Count} expired tokens deleted", deletedCount);
            }
            else
            {
                _logger.LogInformation("Token cleanup completed. No expired tokens found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cleaning up expired tokens");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token Cleanup Service is stopping...");
        return base.StopAsync(cancellationToken);
    }
}
