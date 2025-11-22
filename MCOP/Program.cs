using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus;
using MCOP.Utils;
using Serilog;
using MCOP.Core.Common.Booru;
using MCOP.Core.Services.Singletone;
using MCOP.Data;
using MCOP.Services;
using Microsoft.EntityFrameworkCore;
using Polly.Extensions.Http;
using Polly;
using MCOP.Extensions;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Commands;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using MCOP.Core.Services.Background;
using DSharpPlus.Extensions;
using MCOP.EventListeners;
using MCOP.Core.Services.Scoped.AI;
using MCOP.Core.Services.Scoped;
using MCOP.Core.Services.Scoped.OAuth;

var builder = WebApplication.CreateBuilder(args);

ConfigurationService config = new ConfigurationService();
await config.LoadConfigAsync();

Log.Logger = LogExt.CreateLogger(config.CurrentConfiguration);

var intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers
    | DiscordIntents.MessageContents | TextCommandProcessor.RequiredIntents
    | SlashCommandProcessor.RequiredIntents;

InteractivityConfiguration interactivityConfiguration = new InteractivityConfiguration
{
    PaginationBehaviour = PaginationBehaviour.WrapAround,
    PaginationDeletion = PaginationDeletion.KeepEmojis,
    PaginationEmojis = new PaginationEmojis(),
    PollBehaviour = PollBehaviour.KeepEmojis,
    Timeout = TimeSpan.FromMinutes(1),
};

TextCommandProcessor textCommandProcessor = new(new()
{
    PrefixResolver = new DefaultPrefixResolver(false, config.CurrentConfiguration.Prefix).ResolvePrefixAsync
});

SlashCommandProcessor slashCommandProcessor = new() { };

textCommandProcessor.RegisterConverters();
slashCommandProcessor.RegisterConverters();

Log.Information("Initializing services...");

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger, dispose: true);
builder.Services
    .AddMemoryCache()
    .AddSingleton(config)
    .AddSingleton(Log.Logger)
    .AddDbContextFactory<McopDbContext>(options => options.UseSqlite($"Data Source={config.CurrentConfiguration.DatabaseConfig.DatabaseName}.db;Foreign Keys=True"))
    .AddScoped<MessageListeners>()
    .AddScoped<ClientListeners>()
    .AddScoped<CommandListeners>()
    .AddScoped<ReactionsListeners>()
    .AddSingleton<CooldownService>()
    .AddSingleton<DuelService>()
    .AddSingleton<ILockingService, LockingService>()
    .AddDiscordClient(config.CurrentConfiguration.Token, intents)
    .Configure<DiscordConfiguration>(setup =>
    {
        setup.LogUnknownEvents = false;
    })
    .AddScoped<IDiscordOAuthService, DiscordOAuthService>()
    .AddScoped<IAIService, AIService>()
    .AddScoped<IApiLimitService, ApiLimitService>()
    .AddScoped<IBotStatusesService, BotStatusesService>()
    .AddScoped<IGuildConfigService, GuildConfigService>()
    .AddScoped<IDiscordMessageService, DiscordMessageService>()
    .AddScoped<IGuildMessageService, GuildMessageService>()
    .AddScoped<IRoleApplicationService, RoleApplicationService>()
    .AddScoped<IGuildRoleService, GuildRoleService>()
    .AddScoped<IReactionService, ReactionService>()
    .AddScoped<IGuildUserStatsService, GuildUserStatsService>()
    .AddScoped<IImageHashService, ImageHashService>()
    .AddScoped<IImageVerificationChannelService, ImageVerificationChannelService>()
    .AddScoped<IAppUserService, AppUserService>()
    .AddDiscordEvents()
    .AddInteractivityExtension(interactivityConfiguration)
    .AddCommandsExtension((serviceProvider, extension) =>
    {
        extension.AddProcessor(textCommandProcessor);
        extension.AddProcessor(slashCommandProcessor);
        extension.AddCommands(typeof(Program).Assembly);
        Listeners.RegisterCommandsEvent(extension);
    })
    .AddMemoryCache()
    .AddSharedServices()
    .AddControllers();

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

builder.Services.AddHttpClient("sankaku", client =>
{
    client.Timeout = TimeSpan.FromMinutes(3);
    client.BaseAddress = new Uri("https://sankakuapi.com");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("MCOP/1.0 (by georj)");
}).AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient("e621", client =>
{
    client.BaseAddress = new Uri("https://e621.net");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("MCOP/1.0 (by georj)");
}).AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient("gelbooru", client =>
{
    client.BaseAddress = new Uri("https://gelbooru.com");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("MCOP/1.0 (by georj)");
}).AddPolicyHandler(retryPolicy);

builder.Services.AddSingleton(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    Sankaku sankaku = new Sankaku(httpClientFactory.CreateClient("sankaku"));
    sankaku.AuthorizeAsync("georj", config.CurrentConfiguration.SankakuPassword ?? string.Empty).GetAwaiter().GetResult();
    return sankaku;
});

builder.Services.AddSingleton(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    E621 e621 = new E621(config.CurrentConfiguration.E621HashPassword ?? string.Empty, httpClientFactory.CreateClient("e621"));
    return e621;
});

builder.Services.AddSingleton(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    Gelbooru gelbooru = new Gelbooru(httpClientFactory.CreateClient("gelbooru"));
    return gelbooru;
});

builder.Services.AddHostedService<BotBackgroundService>();
builder.Services.AddHostedService<PeriodicTasksBackgroundService>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuer = config.CurrentConfiguration.JwtConfig.Issuer,
            ValidAudience = config.CurrentConfiguration.JwtConfig.Audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(config.CurrentConfiguration.JwtConfig.Key!)
            )
        };
    });

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    if (builder.Environment.IsDevelopment())
        serverOptions.ListenLocalhost(5000);
    else
        serverOptions.ListenLocalhost(5001);
});

var app = builder.Build();

app.UseCors(builder =>
    builder.WithOrigins("http://localhost:5173", "https://mistercop.top")
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials());

// Миграции базы данных
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IDbContextFactory<McopDbContext>>();
    await db.CreateDbContext().Database.MigrateAsync();
}

app.MapControllers();
app.MapGet("/", () => "MCOP Bot is running!");
app.Run();