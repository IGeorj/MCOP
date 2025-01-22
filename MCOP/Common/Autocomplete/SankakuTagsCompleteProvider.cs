using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using MCOP.Core.Common.Booru;

public class SankakuTagsCompleteProvider : IAutoCompleteProvider
{
    private readonly Sankaku _sankaku;

    public SankakuTagsCompleteProvider(Sankaku sankaku) => _sankaku = sankaku;

    public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        if (context.UserInput is not null)
        {
            var tags = await _sankaku.GetSuggestionsAsync(context.UserInput);
            return await ValueTask.FromResult(tags);
        }

        return [];
    }
}