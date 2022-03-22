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
                var item = POEService.CreateArmour(100, 100, 100, Common.ArmourType.BodyArmour);

                //Generate Image
                await item.GenerateImageAsync();

                var embed = new DiscordEmbedBuilder();
                embed.WithTitle("Название похуй, потом добавлю");
                embed.WithImageUrl($"attachment://generate.png");
                embed.AddField("Evasion", item.EvasionRating.ToString());
                embed.AddField("Armour", item.ArmourRating.ToString());
                embed.AddField("EnergyShield", item.EnergyShield.ToString());

                using FileStream fstream = File.OpenRead("Images/POE/generate.png");
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
