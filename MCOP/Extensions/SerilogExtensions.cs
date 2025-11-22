using MCOP.Utils;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace MCOP.Extensions;

internal static class LogExt
{
    public static Logger CreateLogger(BotConfiguration cfg)
    {
        string template = cfg.CustomLogTemplate
            ?? "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz}] [{Level:u3}] {Message:l}{NewLine}{Exception}";

        LoggerConfiguration lcfg = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(cfg.LogLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http", LogEventLevel.Error)
            .WriteTo.Console(outputTemplate: template)
            ;

        if (cfg.LogToFile)
        {
            lcfg = lcfg.WriteTo.File(
                cfg.LogPath,
                cfg.LogLevel,
                outputTemplate: template,
                rollingInterval: cfg.RollingInterval,
                buffered: cfg.UseBufferedFileLogger,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: cfg.MaxLogFiles
            );
        }

        return lcfg.CreateLogger();
    }
}