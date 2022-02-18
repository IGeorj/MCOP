using System.Reflection;
using MCOP.Database;
using MCOP.EventListeners;
using MCOP.Exceptions;
using MCOP.Extensions;
using MCOP.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using DSharpPlus.SlashCommands;

namespace MCOP;

public sealed class Bot
{
    public ServiceProvider Services => this.services ?? throw new BotUninitializedException();
    public BotConfigService Config => this.config ?? throw new BotUninitializedException();
    public BotDbContextBuilder Database => this.database ?? throw new BotUninitializedException();
    public DiscordClient Client => this.client ?? throw new BotUninitializedException();
    public InteractivityExtension Interactivity => this.interactivity ?? throw new BotUninitializedException();
    public CommandsNextExtension CNext => this.cnext ?? throw new BotUninitializedException();
    public SlashCommandsExtension CSlash => this.cslash ?? throw new BotUninitializedException();
    public IReadOnlyDictionary<string, Command> Commands => this.commands ?? throw new BotUninitializedException();

    private readonly BotConfigService? config;
    private readonly BotDbContextBuilder? database;
    private DiscordClient? client;
    private ServiceProvider? services;
    private InteractivityExtension? interactivity;
    private SlashCommandsExtension? cslash;
    private CommandsNextExtension? cnext;
    private IReadOnlyDictionary<string, Command>? commands;


    public Bot(BotConfigService cfg, BotDbContextBuilder dbb)
    {
        this.config = cfg;
        this.database = dbb;
    }

    public async Task DisposeAsync()
    {
        await this.Client.DisconnectAsync();
        this.Client.Dispose();
        await this.Services.DisposeAsync();
    }


    public async Task StartAsync()
    {
        Log.Information("Initializing the bot...");

        this.client = this.SetupClient();

        this.services = this.SetupServices();
        this.cnext = this.SetupCommands();
        this.cslash = this.SetupSlashCommands();
        this.UpdateCommandList();

        this.interactivity = this.SetupInteractivity();

        Listeners.FindAndRegister(this);

        await this.Client.ConnectAsync();
    }

    public void UpdateCommandList()
    {
        this.commands = this.CNext.GetRegisteredCommands()
            .Where(cmd => cmd.Parent is null)
            .SelectMany(cmd => cmd.Aliases.Select(alias => (alias, cmd)).Concat(new[] { (cmd.Name, cmd) }))
            .ToDictionary(tup => tup.Item1, tup => tup.cmd);
    }


    private DiscordClient SetupClient()
    {
        var cfg = new DiscordConfiguration {
            Token = this.Config.CurrentConfiguration.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            LargeThreshold = 500,
            ShardCount = 1,
            LoggerFactory = new SerilogLoggerFactory(dispose: true),
            Intents = DiscordIntents.All,
        };

        var client = new DiscordClient(cfg);
        client.Ready += (s, e) => {
            Log.Information("Client ready!");
            return Task.CompletedTask;
        };

        return client;
    }

    private ServiceProvider SetupServices()
    {
        Log.Information("Initializing services...");
        return new ServiceCollection()
            .AddSingleton(this.Config)
            .AddSingleton(this.Database)
            .AddSingleton(this.Client)
            .AddSharedServices()
            .BuildServiceProvider()
            .Initialize()
            ;
    }

    private SlashCommandsExtension SetupSlashCommands()
    {
        var slCfg = new SlashCommandsConfiguration
        {
            Services = this.Services,
        };

        SlashCommandsExtension slash = Client.UseSlashCommands(slCfg);
        Log.Debug("Registering SlashCommands...");
        var assembly = Assembly.GetExecutingAssembly();
    
        // Guilds
        // slash.RegisterCommands<ApplicationCommandModule>(323487778220277761);

        // Global
        slash.RegisterCommands(assembly);
        return slash;
    }

    private CommandsNextExtension SetupCommands()
    {
        var cnCfg = new CommandsNextConfiguration {
            CaseSensitive = false,
            EnableDefaultHelp = true,
            EnableDms = true,
            EnableMentionPrefix = true,
            IgnoreExtraArguments = false,
            PrefixResolver = m => Task.FromResult(m.GetStringPrefixLength(this.Config.CurrentConfiguration.Prefix)),
            Services = this.Services,
        };

        CommandsNextExtension cnext = Client.UseCommandsNext(cnCfg);

        Log.Debug("Registering commands...");
        var assembly = Assembly.GetExecutingAssembly();

        cnext.RegisterCommands(assembly);
        cnext.RegisterConverters(assembly);

        return cnext;
    }

    private InteractivityExtension SetupInteractivity()
    {
        return this.Client.UseInteractivity(new InteractivityConfiguration {
            PaginationBehaviour = PaginationBehaviour.WrapAround,
            PaginationDeletion = PaginationDeletion.KeepEmojis,
            PaginationEmojis = new PaginationEmojis(),
            PollBehaviour = PollBehaviour.KeepEmojis,
            Timeout = TimeSpan.FromMinutes(1)
        });
    }
}
