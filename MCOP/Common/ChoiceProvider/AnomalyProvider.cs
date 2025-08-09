using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using MCOP.Services.Duels.Anomalies;
using MCOP.Services.Duels.Anomalies.PokerAnomaly;

namespace MCOP.Common.ChoiceProvider
{
    public sealed class AnomalyProvider : IChoiceProvider
    {
        public const string NoAnomaly = "NoAnomaly";
        public const string Random = "Random";

        public static readonly IReadOnlyList<DiscordApplicationCommandOptionChoice> AnomalyChoices =
        [
            new DiscordApplicationCommandOptionChoice("Без аномалий", NoAnomaly),
            new DiscordApplicationCommandOptionChoice("Рандом", Random),
            new DiscordApplicationCommandOptionChoice("Двойной урон", nameof(DoubleDamageAnomaly)),
            new DiscordApplicationCommandOptionChoice("Инста килл", nameof(InstantWinAnomaly)),
            new DiscordApplicationCommandOptionChoice("Ядовитая", nameof(PoisonAnomaly)),
            new DiscordApplicationCommandOptionChoice("Доджи", nameof(DodgeAnomaly)),
            new DiscordApplicationCommandOptionChoice("Харакири", nameof(HarakiriAnomaly)),
            new DiscordApplicationCommandOptionChoice("Элементальная", nameof(ElementalAnomaly)),
            new DiscordApplicationCommandOptionChoice("Контратака", nameof(CounterAttackAnomaly)),
            new DiscordApplicationCommandOptionChoice("Перенаправление", nameof(SelfDamageAnomaly)),
            new DiscordApplicationCommandOptionChoice("Второй шанс", nameof(SecondChanceAnomaly)),
            new DiscordApplicationCommandOptionChoice("Покер", nameof(PokerAnomaly)),
        ];

        public async ValueTask<IEnumerable<DiscordApplicationCommandOptionChoice>> ProvideAsync(CommandParameter parameter) =>
            await ValueTask.FromResult(AnomalyChoices);
    }
}
