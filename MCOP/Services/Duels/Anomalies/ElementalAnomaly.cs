using Humanizer;
using MCOP.Core.Common;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;

namespace MCOP.Services.Duels.Anomalies
{
    public class ElementalAnomaly : DuelAnomaly
    {
        private enum Element
        {
            [Description("Огонь 🔥")] Fire,
            [Description("Вода 💧")] Water,
            [Description("Земля 🌱")] Earth,
            [Description("Воздух 🌪️")] Air
        }

        private static readonly Dictionary<Element, Element> ElementAdvantages = new Dictionary<Element, Element>
        {
            { Element.Fire, Element.Air },
            { Element.Water, Element.Fire },
            { Element.Earth, Element.Water },
            { Element.Air, Element.Earth }
        };

        public ElementalAnomaly()
        {
            Name = "Элементарно";
            Description = "Каждый раунд игроки получают случайные стихии, которые могут превосходить друг друга!";
        }

        public override void ApplyEffect(Duel duel)
        {
            duel.OnDamageCalculated += (attacker, defender, damage) =>
            {
                Element attackerElement = GetRandomElement();
                Element defenderElement = GetRandomElement();

                string attackerElementName = attackerElement.Humanize();
                string defenderElementName = defenderElement.Humanize();

                duel.LastActionString = $"{attacker.Name} использует {attackerElementName}\n{defender.Name} использует {defenderElementName}.";

                if (ElementAdvantages[attackerElement] == defenderElement)
                {
                    damage *= 2;
                    duel.LastActionString += $"\n{attacker.Name} пробивает двойным уроном на {damage}! 💥";
                }
                else if (ElementAdvantages[defenderElement] == attackerElement)
                {
                    damage = 0;
                    duel.LastActionString += $"\n{defender.Name} поглощает урон! 🛡️";
                }
                else
                {
                    duel.LastActionString += $"\n{attacker.Name} наносит {damage} урона. ⚔️";
                }

                defender.ApplyDamage(damage);
            };
        }

        private Element GetRandomElement()
        {
            return new SafeRandom().ChooseRandomEnumValue<Element>();
        }
    }
}