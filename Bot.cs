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
using DSharpPlus.VoiceNext;
using MCOP.Modules.Nsfw.Common;

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
    public VoiceNextExtension VNext => this.vnext ?? throw new BotUninitializedException();
    public IReadOnlyDictionary<string, Command> Commands => this.commands ?? throw new BotUninitializedException();

    private readonly BotConfigService? config;
    private readonly BotDbContextBuilder? database;
    private DiscordClient? client;
    private ServiceProvider? services;
    private InteractivityExtension? interactivity;
    private SlashCommandsExtension? cslash;
    private CommandsNextExtension? cnext;
    private VoiceNextExtension? vnext;
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

        this.services = await this.SetupServicesAsync();
        this.cnext = this.SetupCommands();
        this.cslash = this.SetupSlashCommands();
        this.UpdateCommandList();

        this.interactivity = this.SetupInteractivity();
        this.vnext = this.SetupVoiceNext();

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

    private async Task<ServiceProvider> SetupServicesAsync()
    {
        Log.Information("Initializing services...");
        ServiceCollection services = new ServiceCollection();

        services.AddSingleton(Config)
            .AddSingleton(Database)
            .AddSingleton(Client)
            .AddSharedServices();

        services.AddHttpClient("sankaku", client =>
        {
            client.BaseAddress = new Uri("https://capi-v2.sankakucomplex.com");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MCOP/1.0 (by georj)");
        });

        services.AddSingleton(provider => 
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            Sankaku sankaku = new Sankaku(httpClientFactory.CreateClient("sankaku"));
            sankaku.AuthorizeAsync("georj", Config.CurrentConfiguration.SankakuPassword ?? string.Empty).GetAwaiter().GetResult();
            return sankaku;
        });

        ServiceProvider provider = services
            .BuildServiceProvider()
            .Initialize();

        return provider;
    }

    private SlashCommandsExtension SetupSlashCommands()
    {
        var slCfg = new SlashCommandsConfiguration
        {
            Services = this.Services,
        };

        SlashCommandsExtension slash = Client.UseSlashCommands(slCfg);
        var assembly = Assembly.GetExecutingAssembly();
        // slash.RegisterCommands<ApplicationCommandModule>(323487778220277761);
        // slash.RegisterCommands<ApplicationCommandModule>(226812644537925642);
        // slash.RegisterCommands(assembly, 323487778220277761);
        // slash.RegisterCommands(assembly, 226812644537925642);
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

    private VoiceNextExtension SetupVoiceNext()
    {
        Log.Information("Initializing VoiceNext...");
        return this.Client.UseVoiceNext();
    }

}
