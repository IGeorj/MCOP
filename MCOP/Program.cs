using System.Diagnostics;
using System.Reflection;
using MCOP.Core.Services.Shared;
using MCOP.Data;
using MCOP.Extensions;
using MCOP.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP;

internal static class Program
{
    public static string ApplicationName { get; }
    public static string ApplicationVersion { get; }

    internal static Bot? Bot { get; set; }
    private static PeriodicTasksService? PeriodicService { get; set; }


    static Program()
    {
        AssemblyName info = Assembly.GetExecutingAssembly().GetName();
        ApplicationName = info.Name ?? typeof(Program).Name;
        ApplicationVersion = $"v{info.Version?.ToString() ?? "<unknown>"}";
    }


    internal static async Task Main(string[] _)
    {
        PrintBuildInformation();

        try {
            ConfigurationService cfg = await LoadBotConfigAsync();
            Log.Logger = LogExt.CreateLogger(cfg.CurrentConfiguration);
            Log.Information("Logger created.");

            await InitializeDatabaseAsync(cfg);

            await StartAsync(cfg);
            Log.Information("Booting complete!");

            await Task.Delay(Timeout.Infinite);
        } catch (TaskCanceledException) {
            Log.Information("Shutdown signal received!");
        } catch (Exception e) {
            Log.Fatal(e, "Critical exception occurred");
            Environment.ExitCode = 1;
        } finally {
            await DisposeAsync();
        }

        Log.Information("Powering off");
        Log.CloseAndFlush();
        Environment.Exit(Environment.ExitCode);
    }

    public static Task Stop(int exitCode = 0, TimeSpan? after = null)
    {
        Environment.ExitCode = exitCode;
        return Task.CompletedTask;
    }


    #region Setup
    private static void PrintBuildInformation()
    {
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        Console.WriteLine($"{ApplicationName} {ApplicationVersion} ({fileVersionInfo.FileVersion})");
        Console.WriteLine();
    }

    private static async Task<ConfigurationService> LoadBotConfigAsync()
    {
        Console.Write("Loading configuration... ");

        var cfg = new ConfigurationService();
        await cfg.LoadConfigAsync();

        Console.Write("\r");
        return cfg;
    }

    private static async Task InitializeDatabaseAsync(ConfigurationService cfg)
    {
        Log.Information("Testing database context creation");
        using (McopDbContext db = new McopDbContext()) {
            IEnumerable<string> pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            await db.Database.MigrateAsync();
        }
    }

    private static Task StartAsync(ConfigurationService cfg)
    {
        Bot = new Bot(cfg);
        PeriodicService = new PeriodicTasksService(Bot, cfg.CurrentConfiguration);
        return Bot.StartAsync();
    }

    private static async Task DisposeAsync()
    {
        Log.Information("Cleaning up ...");

        PeriodicService?.Dispose();
        if (Bot is { })
            await Bot.DisposeAsync();

        Log.Information("Cleanup complete");
    }
    #endregion
}
