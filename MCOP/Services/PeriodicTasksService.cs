using DSharpPlus.Entities;
using MCOP.Core.Services.Booru;
using MCOP.Core.Services.Scoped;
using MCOP.Core.Services.Shared;
using MCOP.Core.Services.Shared.Common;
using MCOP.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MCOP.Services;

public sealed class PeriodicTasksService : IDisposable
{
    #region Callbacks
    private static void BotActivityChangeCallback(object? _)
    {
        if (_ is Bot bot)
        {
            if (bot.Client is null || bot.Client.CurrentUser is null)
            {
                Log.Error("BotActivityChangeCallback detected null client/user - this should not happen but is not nececarily an error");
                return;
            }

            ActivityService bas = bot.Services.GetRequiredService<ActivityService>();

            try
            {
                AsyncExecutionService async = bot.Services.GetRequiredService<AsyncExecutionService>();
                BotStatus? status = async.Execute(bas.GetRandomStatusAsync());
                if (status is null)
                    Log.Warning("No extra bot statuses present in the database.");

                DiscordActivity activity = status is { }
                    ? new DiscordActivity(status.Status, status.Activity)
                    : new DiscordActivity($"Происходит тестирование", DiscordActivityType.Playing);

                async.Execute(bot.Client!.UpdateStatusAsync(activity));
                Log.Information("Changed bot status to {ActivityType} {ActivityName}", activity.ActivityType, activity.Name);
            }
            catch (Exception e)
            {
                Log.Error(e, "An error occured during activity change");
            }
        }
        else
        {
            Log.Error("BotActivityChangeCallback failed to cast sender");
        }
    }

    private static async void DailyTopCallback(object? _)
    {
        if (_ is Bot bot)
        {
            if (bot.Client is null)
            {
                Log.Error("BaseCallback detected null client - this should not happen");
                return;
            }

            try
            {
                SankakuService sankaku = bot.Services.GetRequiredService<SankakuService>();
                GuildService guildService = bot.Services.GetRequiredService<GuildService>();
                var guildConfigs = await guildService.GetGuildConfigsWithLewdChannelAsync();
                List<DiscordChannel> channels = new List<DiscordChannel>();
                foreach (var config in guildConfigs)
                {
                    if (bot.Client.Guilds.ContainsKey(config.GuildId) && config.LewdChannelId is not null)
                    {
                        try
                        {
                            channels.Add(await bot.Client.GetChannelAsync(config.LewdChannelId.Value));
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, $"Cannot send daily Guild: {config.GuildId}, Channel: {config.LewdChannelId.Value}");
                            continue;
                        }
                    }
                }
                await sankaku.SendDailyTopToChannelsAsync(channels);

            }
            catch (Exception e)
            {
                Log.Error(e, "An error occured during BaseCallback timer callback");
            }
        }
        else
        {
            Log.Error("BaseCallback failed to cast sender");
        }
    }
    #endregion

    #region Timers
    private Timer BotStatusUpdateTimer { get; set; }
    private Timer DailyTopTimer { get; set; }
    #endregion


    public PeriodicTasksService(Bot bot, BotConfiguration cfg)
    {
        DateTime now = DateTime.Now;
        DateTime postTime = new DateTime(now.Year, now.Month, now.Day, 20, 00, 00);
        if (now.Hour >= 20)
        {
            postTime = postTime.AddDays(1);
        }
        TimeSpan interval = postTime.Subtract(now);

        this.DailyTopTimer = new Timer(DailyTopCallback, bot, interval, TimeSpan.FromHours(24));
        this.BotStatusUpdateTimer = new Timer(BotActivityChangeCallback, bot, TimeSpan.FromSeconds(10), TimeSpan.FromHours(1));
    }


    public void Dispose()
    {
        this.BotStatusUpdateTimer.Dispose();
        this.DailyTopTimer.Dispose();
    }
}
