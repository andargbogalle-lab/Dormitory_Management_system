using dprmitory.Data;

namespace dprmitory.Services
{
    /// <summary>
    /// Background service that automatically resets a student's availability status
    /// from Present back to Absent exactly 24 hours after they were marked Present.
    /// Checks every minute so the reset happens on time for each student individually.
    /// </summary>
    public class DailyResetService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyResetService> _logger;

        // Check every 60 seconds for expired Present statuses
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

        public DailyResetService(IServiceScopeFactory scopeFactory, ILogger<DailyResetService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DailyResetService started. Checking every {Minutes} minute(s) for expired Present statuses.",
                CheckInterval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(CheckInterval, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var studentRepo = scope.ServiceProvider.GetRequiredService<StudentRepository>();

                    // Reset any student whose Present status is older than 24 hours
                    studentRepo.ResetExpiredPresentStatus();

                    _logger.LogDebug("Checked for expired Present statuses at {Time}.",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during expired Present status check.");
                }
            }
        }
    }
}
