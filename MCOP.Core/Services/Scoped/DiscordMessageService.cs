using DSharpPlus;
using DSharpPlus.Entities;
using MCOP.Common;
using MCOP.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net.Mail;

namespace MCOP.Core.Services.Scoped
{
    public interface IDiscordMessageService
    {
        Task SendRoleGrantedMessageAsync(ulong guildId, DiscordMember user, DiscordRole role, int newLevel, DiscordChannel channel);
        Task SendRoleRevokedMessageAsync(ulong guildId, DiscordMember user, DiscordRole role, DiscordChannel channel);
        Task<bool> IsLevelUpMessagesEnabledAsync(ulong guildId);
        Task<string> BuildLevelUpMessageAsync(ulong guildId, DiscordMember user, DiscordRole role, int newLevel);
        Task SendCopyFoundMessageAsync(DiscordUser user, DiscordChannel channel, DiscordMessage message, DiscordMessage messageFromHash, double difference, string? attachmentUrl);
    }

    public class DiscordMessageService : IDiscordMessageService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;
        private readonly IGuildConfigService _guildConfigService;
        private readonly DiscordClient _discordClient;

        public DiscordMessageService(IDbContextFactory<McopDbContext> contextFactory, IGuildConfigService guildConfigService, DiscordClient discordClient)
        {
            _contextFactory = contextFactory;
            _guildConfigService = guildConfigService;
            _discordClient = discordClient;
        }

        public async Task SendRoleGrantedMessageAsync(ulong guildId, DiscordMember user, DiscordRole role, int newLevel, DiscordChannel channel)
        {
            if (!await IsLevelUpMessagesEnabledAsync(guildId))
                return;

            string content = await BuildLevelUpMessageAsync(guildId, user, role, newLevel);
            if (!string.IsNullOrWhiteSpace(content))
            {
                var messageBuilder = new DiscordMessageBuilder().WithContent(content);
                await messageBuilder.SendAsync(channel);
            }
        }

        public async Task SendRoleRevokedMessageAsync(ulong guildId, DiscordMember user, DiscordRole role, DiscordChannel channel)
        {
            if (!await IsLevelUpMessagesEnabledAsync(guildId))
                return;

            string content = $"<@{user.Id}> Заскамили на роль, убираем <@&{role.Id}>";
            var messageBuilder = new DiscordMessageBuilder().WithContent(content);
            await messageBuilder.SendAsync(channel);
        }

        public async Task SendCopyFoundMessageAsync(
            DiscordUser user,
            DiscordChannel channel,
            DiscordMessage message,
            DiscordMessage messageFromHash,
            double difference,
            string? attachmentUrl)
        {
            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(
                    new DiscordContainerComponent(
                        [
                            new DiscordSectionComponent(
                                new DiscordTextDisplayComponent(
                                    $"""
                                    ## Найдено совпадение
                                    **Новое**: {user.Username}
                                    **Прошлое**: {messageFromHash.Author?.Username ?? ""}
                                    **Совпадение:** {difference:0.00} %
                                    """
                                    ), 
                                new DiscordThumbnailComponent(attachmentUrl ?? "")
                            ),
                            new DiscordActionRowComponent(
                                [
                                    new DiscordLinkButtonComponent(message.JumpLink.ToString(), "Новое"),
                                    new DiscordLinkButtonComponent(messageFromHash.JumpLink.ToString(), "Прошлое"),
                                    new DiscordButtonComponent(
                                        DiscordButtonStyle.Success,
                                        GlobalNames.Buttons.RemoveMessage + $"UID:{user.Id}",
                                        "Понял",
                                        false,
                                        new DiscordComponentEmoji(DiscordEmoji.FromName(_discordClient, ":heavy_check_mark:" ))),
                                ]
                            )
                        ]
                    )
                );

            await channel.SendMessageAsync(messageBuilder);
        }

        public Task<bool> IsLevelUpMessagesEnabledAsync(ulong guildId)
            => _guildConfigService.IsLevelUpMessagesEnabledAsync(guildId);

        public async Task<string> BuildLevelUpMessageAsync(ulong guildId, DiscordMember user, DiscordRole role, int newLevel)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var config = await context.GuildConfigs.FindAsync(guildId);
                var roleEntity = await context.GuildRoles.FirstOrDefaultAsync(r => r.GuildId == guildId && r.Id == role.Id);
                var template = !string.IsNullOrWhiteSpace(roleEntity?.LevelUpMessageTemplate)
                    ? roleEntity.LevelUpMessageTemplate
                    : config?.LevelUpMessageTemplate;

                if (string.IsNullOrWhiteSpace(template))
                    return string.Empty;

                return template
                    .Replace("{user}", $"<@{user.Id}>")
                    .Replace("{role}", $"<@&{role.Id}>")
                    .Replace("{level}", newLevel.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error building level-up message for guild {GuildId}", guildId);
                return string.Empty;
            }
        }
    }
}
