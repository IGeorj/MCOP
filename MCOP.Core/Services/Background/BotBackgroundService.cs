using DSharpPlus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MCOP.Core.Services.Background
{
    public sealed class BotBackgroundService : BackgroundService
    {
        private readonly DiscordClient _client;
        private readonly ILogger<BotBackgroundService> _logger;
        private readonly ManualResetEvent _shutdownSignal = new(false);
        public BotBackgroundService(
            DiscordClient client,
            ILogger<BotBackgroundService> logger)
        {
            _client = client;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Connecting to Discord...");
                await _client.ConnectAsync();

                await Task.Run(_shutdownSignal.WaitOne, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Bot crashed");
                throw;
            }
        }

        public async Task StopBotAsync()
        {
            _logger.LogInformation("Manual shutdown requested");
            _shutdownSignal.Set();
            await StopAsync(CancellationToken.None);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Disconnecting from Discord...");
            await _client.DisconnectAsync();
            _client.Dispose();
            await base.StopAsync(cancellationToken);
        }
    }
}
