

using ExpenseVista.API.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVista.API.Services.Background
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TokenCleanupService> _logger;

        // Configuration: How often to run? (every 24 hours)
        private readonly TimeSpan _period = TimeSpan.FromHours(24);

        public TokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token Cleanup Service is starting.");

            using PeriodicTimer timer = new PeriodicTimer(_period);

            // This loop runs every 24 hours until the app shuts down
            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupTokensAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while cleaning up tokens.");
                }
            }
        }

        private async Task CleanupTokensAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleaning up expired refresh tokens...");

            //Create a temporary scope and resolve the DbContext, but this service is Singleton
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Define Logic: Delete tokens that expired more than 30 days ago
                var cutoffDate = DateTime.UtcNow.AddDays(-30);

                // EXECUTE DELETE is very fast because it runs a raw SQL DELETE command.
                var count = await dbContext.RefreshTokens
                    .Where(t => t.Expires < cutoffDate)
                    .ExecuteDeleteAsync(stoppingToken);

                _logger.LogInformation($"Token Cleanup complete. Deleted {count} old tokens.");
            }
        }
    }
}