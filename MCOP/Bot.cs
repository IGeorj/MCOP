using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using MCOP.Core.Common.Booru;
using MCOP.Core.Services.Singletone;
using MCOP.Data;
using MCOP.Extensions;
using MCOP.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace MCOP;

public sealed class Bot
{
    public IServiceProvider Services { get; private set; } = null!;
    public Core.Services.Shared.ConfigurationService Config { get; private set; } = null!;
    public DiscordClient Client { get; private set; } = null!;

    public Bot(Core.Services.Shared.ConfigurationService cfg)
    {
        Config = cfg;

        Log.Information("Initializing the bot...");

        SetupClient();
    }

    public async Task DisposeAsync()
    {
        await Client.DisconnectAsync();
        Client.Dispose();
    }

    public async Task StartAsync()
    {
        await Client.ConnectAsync();
    }

    private void SetupClient()
    {
        var intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.MessageContents 
            | TextCommandProcessor.RequiredIntents | SlashCommandProcessor.RequiredIntents;

        var clientBuilder = DiscordClientBuilder.CreateDefault(Config.CurrentConfiguration.Token, intents);

        EventListeners.Listeners.RegisterEvents(clientBuilder);

        clientBuilder.ConfigureLogging(logging =>
        {
            logging.AddSerilog(Log.Logger, dispose: true);
        });

        clientBuilder.ConfigureEventHandlers(x =>
        {
            x.HandleSessionCreated((s, e) => { Log.Information("SessionCreated!"); return Task.CompletedTask; });
        });

        clientBuilder.ConfigureServices(serviceConfig =>
        {
            Log.Information("Initializing services...");

            serviceConfig
            .AddSingleton(Config)
            .AddSingleton<CooldownService>()
            .AddSingleton<DuelService>()
            .AddSingleton<ILockingService, LockingService>()
            .AddMemoryCache()
            .AddDbContextFactory<McopDbContext>(options => options.UseSqlite($"Data Source={Config.CurrentConfiguration.DatabaseConfig.DatabaseName}.db;Foreign Keys=True"))
            .AddSharedServices()
            .AddScopedServices();

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            serviceConfig.AddHttpClient("sankaku", client =>
            {
                client.Timeout = TimeSpan.FromMinutes(3);
                client.BaseAddress = new Uri("https://sankakuapi.com");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("MCOP/1.0 (by georj)");
            }).AddPolicyHandler(retryPolicy);
            serviceConfig.AddHttpClient("e621", client =>
            {
                client.BaseAddress = new Uri("https://e621.net");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("MCOP/1.0 (by georj)");
            }).AddPolicyHandler(retryPolicy);
            serviceConfig.AddHttpClient("gelbooru", client =>
            {
                client.BaseAddress = new Uri("https://gelbooru.com");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("MCOP/1.0 (by georj)");
            }).AddPolicyHandler(retryPolicy);

            serviceConfig.AddSingleton(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                Sankaku sankaku = new Sankaku(httpClientFactory.CreateClient("sankaku"));
                sankaku.AuthorizeAsync("georj", Config.CurrentConfiguration.SankakuPassword ?? string.Empty).GetAwaiter().GetResult();
                return sankaku;
            });
            serviceConfig.AddSingleton(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                E621 e621 = new E621(Config.CurrentConfiguration.E621HashPassword ?? string.Empty, httpClientFactory.CreateClient("e621"));
                return e621;
            });
            serviceConfig.AddSingleton(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                Gelbooru gelbooru = new Gelbooru(httpClientFactory.CreateClient("gelbooru"));
                return gelbooru;
            });

            var serviceProvider = serviceConfig.BuildServiceProvider();
            Services = serviceProvider;
            EventListeners.Listeners.RegisterServiceProvider(serviceProvider);
        });

        SetupCommands(clientBuilder);
        SetupInteractivity(clientBuilder);

        Client = clientBuilder.Build();
    }

    private void SetupCommands(DiscordClientBuilder clientBuilder)
    {
        Log.Information("Registering commands...");

        TextCommandProcessor textCommandProcessor = new(new()
        {
            PrefixResolver = new DefaultPrefixResolver(false, Config.CurrentConfiguration.Prefix).ResolvePrefixAsync
        });

        SlashCommandProcessor slashCommandProcessor = new() { };

        textCommandProcessor.RegisterConverters();
        slashCommandProcessor.RegisterConverters();

        var discordClient = clientBuilder.UseCommands((serviceProvider, extension) =>
        {
            extension.AddProcessor(textCommandProcessor);
            extension.AddProcessor(slashCommandProcessor);
            extension.AddCommands(typeof(Program).Assembly);
            EventListeners.Listeners.RegisterCommandsEvent(extension);
        });
    }

    private void SetupInteractivity(DiscordClientBuilder clientBuilder)
    {
        clientBuilder.UseInteractivity(new InteractivityConfiguration
        {
            PaginationBehaviour = PaginationBehaviour.WrapAround,
            PaginationDeletion = PaginationDeletion.KeepEmojis,
            PaginationEmojis = new PaginationEmojis(),
            PollBehaviour = PollBehaviour.KeepEmojis,
            Timeout = TimeSpan.FromMinutes(1),
        });
    }
}
