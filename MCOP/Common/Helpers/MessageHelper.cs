using DSharpPlus.EventArgs;
using DSharpPlus;
using MCOP.Core.Common;
using DSharpPlus.Entities;
using MCOP.Extensions;

namespace MCOP.Common.Helpers
{
    public class MessageHelper
    {
        private static DateTime _lastArgumentReply = DateTime.MinValue;

        public static async Task CheckEveryoneAsync(DiscordClient client, MessageCreatedEventArgs e)
        {
            if (e.Guild.Id == GlobalVariables.McopServerId && e.Message.Content.Contains("@everyone"))
            {
                var member = await e.Guild.GetMemberAsync(e.Author.Id);
                if (member is not null && (!member.IsAdmin() || !member.Permissions.HasPermission(DiscordPermission.MentionEveryone)))
                {
                    await e.Message.DeleteSilentAsync();

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                        .WithAuthor(member.Username, null, member.AvatarUrl)
                        .WithColor(DiscordColor.Yellow)
                        .AddField("Пользователь", $"<@!{member.Id}>", true)
                        .AddField("Модератор", client.CurrentUser.Mention, true)
                        .AddField("Результат", "Вадана роль САСЁШЬ", true)
                        .AddField("Канал", $"<#{e.Channel.Id}>")
                        .AddField("Сообщение", e.Message.Content);

                    DiscordChannel? publicChannel = await e.Guild.GetPublicUpdatesChannelAsync();

                    if (publicChannel is not null)
                    {
                        await publicChannel.SendMessageAsync(embed.Build());
                    }

                    // САСЁШЬ
                    DiscordRole? role = await e.Guild.GetRoleAsync(622772942761361428);

                    if (role is not null)
                    {
                        await member.GrantRoleAsync(role);
                    }
                }
            }
        }

        public static async Task CheckDulyaAsync(DiscordClient client, MessageCreatedEventArgs e)
        {
            if (e.Guild.Id != GlobalVariables.McopServerId) return;

            bool isContainsArgument = !e.Author.IsBot && e.Message.Content.Contains("<a:mcopArgument:1341651747818705016>");

            if (isContainsArgument && DateTime.UtcNow - _lastArgumentReply >= TimeSpan.FromMinutes(1))
            {
                await SendDulya(client, e);
                return;
            }

            if (e.MentionedUsers.Any(x => x.Id == client.CurrentApplication?.Bot?.Id))
            {
                await SendDulya(client, e);
            }
        }

        private static async Task SendDulya(DiscordClient client, MessageCreatedEventArgs e)
        {
            var parsed = DiscordEmoji.TryFromName(client, ":mcopArgument:", out DiscordEmoji dulyaEmoji);
            if (parsed)
            {
                await e.Channel.SendMessageAsync(dulyaEmoji);
                _lastArgumentReply = DateTime.UtcNow;
            }
        }
    }
}
