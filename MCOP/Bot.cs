using System.Reflection;
using MCOP.Exceptions;
using MCOP.Extensions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using DSharpPlus.SlashCommands;
using MCOP.Data;
using MCOP.Core.Services.Shared;
using MCOP.Core.Common.Booru;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;

namespace MCOP;

public sealed class Bot
{
    public ServiceProvider Services => services ?? throw new BotUninitializedException();
    public ConfigurationService Config => config ?? throw new BotUninitializedException();
    public DiscordClient Client => client ?? throw new BotUninitializedException();
    public InteractivityExtension Interactivity => interactivity ?? throw new BotUninitializedException();
    public CommandsNextExtension CNext => cnext ?? throw new BotUninitializedException();
    public SlashCommandsExtension CSlash => cslash ?? throw new BotUninitializedException();
    public IReadOnlyDictionary<string, Command> Commands => commands ?? throw new BotUninitializedException();

    private readonly ConfigurationService? config;
    private DiscordClient? client;
    private ServiceProvider? services;
    private InteractivityExtension? interactivity;
    private SlashCommandsExtension? cslash;
    private CommandsNextExtension? cnext;
    private IReadOnlyDictionary<string, Command>? commands;


    public Bot(ConfigurationService cfg)
    {
        config = cfg;
    }

    public async Task DisposeAsync()
    {
        await Client.DisconnectAsync();
        Client.Dispose();
        await Services.DisposeAsync();
    }


    public async Task StartAsync()
    {
        Log.Information("Initializing the bot...");
       
        client = SetupClient();

        services = SetupServices();
        cnext = SetupCommands();
        cslash = SetupSlashCommands();
        UpdateCommandList();
        interactivity = SetupInteractivity();
        EventListeners.Listeners.RegisterEvents(this);
        await Client.ConnectAsync();
    }

    public void UpdateCommandList()
    {
        commands = CNext.GetRegisteredCommands()
            .Where(cmd => cmd.Parent is null)
            .SelectMany(cmd => cmd.Aliases.Select(alias => (alias, cmd)).Concat(new[] { (cmd.Name, cmd) }))
            .ToDictionary(tup => tup.Item1, tup => tup.cmd);
    }


    private DiscordClient SetupClient()
    {
        var cfg = new DiscordConfiguration {
            Token = Config.CurrentConfiguration.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            LargeThreshold = 500,
            ShardCount = 1,
            LoggerFactory = new SerilogLoggerFactory(dispose: true),
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.MessageContents,
            LogUnknownEvents = false,
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
        ServiceCollection services = new ServiceCollection();

        services.AddSingleton(Config)
            .AddSingleton(Client)
            .AddDbContext<McopDbContext>(options => options.UseSqlite($"Data Source={Config.CurrentConfiguration.DatabaseConfig.DatabaseName}.db;"), ServiceLifetime.Transient)
            .AddSharedServices()
            .AddScopedClasses();

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        services.AddHttpClient("sankaku", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(3);
            client.BaseAddress = new Uri("https://capi-v2.sankakucomplex.com");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MCOP/1.0 (by georj)");
        }).AddPolicyHandler(retryPolicy);
        services.AddHttpClient("e621", client =>
        {
            client.BaseAddress = new Uri("https://e621.net");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MCOP/1.0 (by georj)");
        }).AddPolicyHandler(retryPolicy);
        services.AddHttpClient("gelbooru", client =>
        {
            client.BaseAddress = new Uri("https://gelbooru.com");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MCOP/1.0 (by georj)");
        }).AddPolicyHandler(retryPolicy);

        services.AddSingleton(provider => 
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            Sankaku sankaku = new Sankaku(httpClientFactory.CreateClient("sankaku"));
            sankaku.AuthorizeAsync("georj", Config.CurrentConfiguration.SankakuPassword ?? string.Empty).GetAwaiter().GetResult();
            return sankaku;
        });
        services.AddSingleton(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            E621 e621 = new E621(Config.CurrentConfiguration.E621HashPassword ?? string.Empty, httpClientFactory.CreateClient("e621"));
            return e621;
        });
        services.AddSingleton(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            Gelbooru gelbooru = new Gelbooru(httpClientFactory.CreateClient("gelbooru"));
            return gelbooru;
        });

        ServiceProvider provider = services
            .BuildServiceProvider();

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
            PrefixResolver = m => Task.FromResult(m.GetStringPrefixLength(Config.CurrentConfiguration.Prefix)),
            Services = this.Services,
        };

        CommandsNextExtension cnext = Client.UseCommandsNext(cnCfg);

        Log.Information("Registering commands...");
        var assembly = Assembly.GetExecutingAssembly();

        cnext.RegisterCommands(assembly);
        cnext.RegisterConverters(assembly);

        return cnext;
    }

    private InteractivityExtension SetupInteractivity()
    {
        return Client.UseInteractivity(new InteractivityConfiguration {
            PaginationBehaviour = PaginationBehaviour.WrapAround,
            PaginationDeletion = PaginationDeletion.KeepEmojis,
            PaginationEmojis = new PaginationEmojis(),
            PollBehaviour = PollBehaviour.KeepEmojis,
            Timeout = TimeSpan.FromMinutes(1),
        });
    }
}
