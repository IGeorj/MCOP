using DSharpPlus;
using DSharpPlus.Entities;
using MCOP.Core.Services.Booru;
using MCOP.Core.Services.Scoped;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MCOP.Core.Services.Background;

public sealed class PeriodicTasksBackgroundService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _services;
    private Timer? _statusTimer;
    private Timer? _dailyTimer;

    public PeriodicTasksBackgroundService(
        ILogger logger,
        IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }
    
    private void InitializeTimers()
    {
        _logger.Information("Creating timers...");

        _statusTimer = new Timer(
            callback: UpdateBotStatus,
            state: null,
            dueTime: TimeSpan.FromSeconds(10),
            period: TimeSpan.FromHours(1));

        var now = DateTime.Now;
        var postTime = new DateTime(now.Year, now.Month, now.Day, 20, 0, 0);
        if (now.Hour >= 20) postTime = postTime.AddDays(1);

        _dailyTimer = new Timer(
            callback: SendDailyTop,
            state: null,
            dueTime: postTime - now,
            period: TimeSpan.FromHours(24));
    }

    private async void UpdateBotStatus(object? state)
    {
        try
        {
            using var scope = _services.CreateScope();
            var services = scope.ServiceProvider;

            var client = services.GetRequiredService<DiscordClient>();
            var statusService = services.GetRequiredService<IBotStatusesService>();

            if (client.CurrentUser == null)
            {
                _logger.Warning("Cannot update status - CurrentUser is null");
                return;
            }

            var status = await statusService.GetRandomStatusAsync();

            DiscordActivity activity = status != null
                ? new DiscordActivity(status.Status, status.Activity)
                : new DiscordActivity("Default Status", DiscordActivityType.Playing);

            await client.UpdateStatusAsync(activity);
            _logger.Information("Updated bot status to {ActivityType} {ActivityName}",
                activity.ActivityType, activity.Name);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating bot status");
        }
    }

    private async void SendDailyTop(object? state)
    {
        try
        {
            using var scope = _services.CreateScope();
            var services = scope.ServiceProvider;

            var client = services.GetRequiredService<DiscordClient>();
            var sankakuService = services.GetRequiredService<SankakuService>();
            var guildConfigService = services.GetRequiredService<IGuildConfigService>();

            // Получаем список каналов для отправки
            var guildConfigs = await guildConfigService.GetGuildConfigsWithLewdChannelAsync();
            var channels = new List<DiscordChannel>();

            foreach (var config in guildConfigs)
            {
                try
                {
                    if (client.Guilds.TryGetValue(config.GuildId, out var guild) &&
                        config.LewdChannelId.HasValue)
                    {
                        var channel = await guild.GetChannelAsync(config.LewdChannelId.Value);
                        channels.Add(channel);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to get channel {ChannelId} in guild {GuildId}",
                        config.LewdChannelId, config.GuildId);
                }
            }

            if (channels.Count == 0)
            {
                _logger.Warning("No valid channels found for daily top");
                return;
            }

            // Отправляем топ в каждый канал
            await sankakuService.SendDailyTopToChannelsAsync(channels);
            _logger.Information("Sent daily top to {Count} channels", channels.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error sending daily top");

            // Повторная попытка через 10 минут при ошибке
            await Task.Delay(TimeSpan.FromMinutes(10));
            SendDailyTop(state);
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        InitializeTimers();
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _statusTimer?.Dispose();
        _dailyTimer?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}