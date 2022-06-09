using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MCOP.Modules.POE.Services;
using MCOP.Services;

namespace MCOP.Modules.POE
{

    [SlashCommandGroup("poe", "POE команды")]
    public sealed class POEModule : ApplicationCommandModule
    {
        public POEItemService POEService { get; set; }


        [SlashCommand("randomitem", "Генерирует рандомный предмет")]
        public async Task GenerateItem(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                var item = POEService.CreateRandomArmour();

                //Generate Image
                string path = item.GenerateImage();

                var embed = new DiscordEmbedBuilder();
                embed.WithTitle(item.Name);
                embed.WithImageUrl($"attachment://generate.png");

                using FileStream fstream = File.OpenRead(path);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(embed)
                    .AddFile("generate.png", fstream)
                );

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
