using MCOP.Database.Models;
using MCOP.Services.Common;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using MCOP.Modules.Nsfw.Services;

namespace MCOP.Services;

public sealed class PeriodicTasksService : IDisposable
{
    #region Callbacks
    private static void BotActivityChangeCallback(object? _)
    {
        if (_ is Bot bot) {
            if (bot.Client is null || bot.Client.CurrentUser is null) {
                Log.Error("BotActivityChangeCallback detected null client/user - this should not happen but is not nececarily an error");
                return;
            }

            BotActivityService bas = bot.Services.GetRequiredService<BotActivityService>();
            if (!bas.StatusRotationEnabled)
                return;

            try {
                BotStatus? status = bas.GetRandomStatus();
                if (status is null)
                    Log.Warning("No extra bot statuses present in the database.");

                DiscordActivity activity = status is { }
                    ? new DiscordActivity(status.Status, status.Activity)
                    : new DiscordActivity($"Происходит тестирование", ActivityType.Playing);

                AsyncExecutionService async = bot.Services.GetRequiredService<AsyncExecutionService>();
                async.Execute(bot.Client!.UpdateStatusAsync(activity));
                Log.Debug("Changed bot status to {ActivityType} {ActivityName}", activity.ActivityType, activity.Name);
            } catch (Exception e) {
                Log.Error(e, "An error occured during activity change");
            }
        } else {
            Log.Error("BotActivityChangeCallback failed to cast sender");
        }
    }

    private static async void DailyTopCallback(object? _)
    {
        if (_ is Bot bot) {
            if (bot.Client is null) {
                Log.Error("BaseCallback detected null client - this should not happen");
                return;
            }

            try {
                SankakuService sankaku = bot.Services.GetRequiredService<SankakuService>();
                DiscordGuild guild = await bot.Client.GetGuildAsync(323487778220277761);
                DiscordChannel channel = guild.GetChannel(857354195866615808);
                await sankaku.SendDailyTopAsync(channel, 80);

            } catch (Exception e) {
                Log.Error(e, "An error occured during BaseCallback timer callback");
            }
        } else {
            Log.Error("BaseCallback failed to cast sender");
        }
    }
    #endregion

    #region Timers
    private Timer BotStatusUpdateTimer { get; set; }
    private Timer DailyTopTimer { get; set; }
    #endregion


    public PeriodicTasksService(Bot bot, BotConfig cfg)
    {
        DateTime now = DateTime.Now;
        DateTime postTime = new DateTime(now.Year, now.Month, now.Day, 20, 00, 00);
        if (now.Hour >= 20)
        {
            postTime = postTime.AddDays(1);
        }
        TimeSpan interval = postTime.Subtract(now);

        this.DailyTopTimer = new Timer(DailyTopCallback, bot, interval, TimeSpan.FromHours(24));
        this.BotStatusUpdateTimer = new Timer(BotActivityChangeCallback, bot, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(10));
    }


    public void Dispose()
    {
        this.BotStatusUpdateTimer.Dispose();
        this.DailyTopTimer.Dispose();
    }
}
