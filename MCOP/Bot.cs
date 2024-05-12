using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using MCOP.Attributes;
using MCOP.Core.Common.Booru;
using MCOP.Core.Services.Shared;
using MCOP.Data;
using MCOP.Exceptions;
using MCOP.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Extensions.Logging;

namespace MCOP;

public sealed class Bot
{
    public ServiceProvider Services => services ?? throw new BotUninitializedException();
    public ConfigurationService Config => config ?? throw new BotUninitializedException();
    public DiscordClient Client => client ?? throw new BotUninitializedException();
    public InteractivityExtension Interactivity => interactivity ?? throw new BotUninitializedException();
    public CommandsExtension CommandsEx => cnext ?? throw new BotUninitializedException();

    private readonly ConfigurationService? config;
    private DiscordClient? client;
    private ServiceProvider? services;
    private InteractivityExtension? interactivity;
    private CommandsExtension? cnext;


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
        cnext = await SetupCommands();
        interactivity = SetupInteractivity();
        EventListeners.Listeners.RegisterEvents(this);
        await Client.ConnectAsync();
    }

    private DiscordClient SetupClient()
    {
        var cfg = new DiscordConfiguration
        {
            Token = Config.CurrentConfiguration.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            LargeThreshold = 500,
            ShardCount = 1,
            LoggerFactory = new SerilogLoggerFactory(dispose: true),
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.MessageContents
            | TextCommandProcessor.RequiredIntents | SlashCommandProcessor.RequiredIntents,
            LogUnknownEvents = false,
        };

        var client = new DiscordClient(cfg);
        client.SessionCreated += (s, e) =>
        {
            Log.Information("SessionCreated!");
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

    private async Task<CommandsExtension> SetupCommands()
    {
        Log.Information("Registering commands...");

        CommandsExtension commandsExtensions = Client.UseCommands(new CommandsConfiguration()
        {
            ServiceProvider = Services,
        });


        TextCommandProcessor textCommandProcessor = new(new()
        {
            PrefixResolver = new DefaultPrefixResolver(Config.CurrentConfiguration.Prefix).ResolvePrefixAsync
        });

        SlashCommandProcessor slashCommandProcessor = new SlashCommandProcessor
        {

        };

        textCommandProcessor.RegisterConverters();
        slashCommandProcessor.RegisterConverters();

        commandsExtensions.AddCommands(typeof(Program).Assembly);
        commandsExtensions.AddCheck<RequireNsfwChannelAttributeCheck>();

        await commandsExtensions.AddProcessorsAsync(textCommandProcessor);
        await commandsExtensions.AddProcessorsAsync(slashCommandProcessor);

        return commandsExtensions;
    }

    private InteractivityExtension SetupInteractivity()
    {
        return Client.UseInteractivity(new InteractivityConfiguration
        {
            PaginationBehaviour = PaginationBehaviour.WrapAround,
            PaginationDeletion = PaginationDeletion.KeepEmojis,
            PaginationEmojis = new PaginationEmojis(),
            PollBehaviour = PollBehaviour.KeepEmojis,
            Timeout = TimeSpan.FromMinutes(1),
        });
    }
}
