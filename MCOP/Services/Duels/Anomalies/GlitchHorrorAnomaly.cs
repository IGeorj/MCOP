using MCOP.Core.Common;

namespace MCOP.Services.Duels.Anomalies
{
    public sealed class GlitchHorrorAnomaly : DuelAnomaly
    {
        private readonly string[] _glitchSymbols = new[]
        {
            "⸮", "⸘", "⚉", "⸑", "⸒", 
            "☠", "◾", "⛧", "⚰", 
            "🜂", "🜄", "🜆", "🜍", "🜏", 
            "⌀", "⌂", "⌇", "⌌", "⌏",
            "▓", "▒", "░", "▉", "▣",
            "�", "�", "�", "�", "�"
        };

        private readonly string[] _creepyMessages = new[]
        {
            "Т̴̖̈̈у̸͕̖̅т̵͕̂͋ ̵̪͊̚ќ̵̯̥т̷͈̈́̒о̷̲̙͝ ̴͖͇̉̆н̸̖͓̆̔и̸̹̽̚б̴̫̈́ӳ̴̬̍д̶̬̾̍ь̶̬͝ ̴͎͍͑е̴̭̉͠с̴̠̐̀т̵̞̥͋͂ь̶̫̈́͘ ̴̙̫̿?̵̪̑̔.̴̗̯͐͌.̴̤̞͆.̷̫̎.̷͔̘͠.̴̪͒",
            "11010000 10100001 11010000 10111111 11010000 10110000 11010001 10000001 11010000 10111000 11010001 10000010 11010000 10110101 101110 101110 101110",
            "Я̷̧̆ ̶̼̽с̶͐ͅл̵̱͕̐̊ы̸͉̿̔ш̴̠̀̋у̷̲̇ ̵̖̙̓▓̸̢̱͆▓̶̨̈́▓̵̮̘̓▓̶̧̳͘▓̴̘͐̃.̴̣̓̕.̶̥͗̎.̵̮̒",
            "М̶н̶е̸ ̶с̶т̵р̸а̴̵ш̸н̴o̵.̸.̷.̶..",
            "К̵̲͖̻̉͋͊͋͂т̷̢̙͔̲́͠ӧ̸̳́͑̅-̷̫̃̅̕͝н̷̪̺̼̘̏͋̏̚и̷̢̫̝̋͜б̵̨͕̊̍̓̚ӱ̶̝̊д̷͇̂̓̔̚ь̸̛͖̎̔,̷̨̹͖́͒͒͜͜ ̴̭̩̼̱̦͠п̷̦̒̂̍̾̔о̵̝̻͉́̄̊̃м̵̤̖̳̒̉̚о̸̗͝г̴̬͇̺͖̈́̀̾и̸̧̼̥̪̎͑͛͆͝т̵͕̦̇͋ё̸̡̮́̓.̶͎̓͊̉́̾.̴̠̝͒̈̋̀.̷̩̺̥̟̠̂̈͘",
            "Т̷̦͝ы̴̦̚ ̵̹̽н̴̦͝е̷̱̕ ̵̙̆с̵̦͝л̴̦̚ы̵̹̽ш̴̦͝и̷̱̕ш̵̙̆ь̵̦͝ ̴̦̚э̵̹̽т̴̦͝о̷̱̕г̵̙̆о̵̦͝?̴̦̚.̵̹̽.̴̦͝.̷̱̕.̵̙̆.̵̦͝",
            "421 442 440 430 445",
            "О̸̗͝н̴̬͇̺͖̈́̀̾и̸̧̼̥̪̎͑͛͆͝ ̵͕̦̇͋ӟ̸̡̮́̓д̶͎̓͊̉́̾е̴̠̝͒̈̋̀с̷̩̺̥̟̠̂̈͘ь̴̦̚.̵̹̽.̴̦͝.̷̱̕.̵̙̆.̵̦͝",
            "Я̶̻̂͗ ̶̠̰̔̀н̴̜̒͘е̶̟̄͝ͅ ̵̮̝̄х̵̧̧͐о̸̟̥͝ч̴̜̱͆͝у̵͔̓̾ ̸̘̳̅͒у̶̖̀м̶̨̪̄͒и̷̖̳̽р̵̨̑̔а̵̠̐т̷̳͌ь̴͖͎͊.̶̟͓̅̀.̵̧̓.̴̧͉̇̎.̷̳̥̇͝",
            "П̴̻̫͑о̴̳͝ч̶̛͔̳͠е̵̻̮̚м̵͓͉̈́́ў̷͚͊ ̷͍͗т̵͚͂͋а̶̼̫̋к̷̧͓̓ ̶͉̍х̵̝̜͂̎о̸̖̀͝л̶̰̽о̸̛͖̀д̷͓̼̋͝н̵̙̈͂о̴̦̲̏?̷̨̓.̸̼̳̾.̷̲̭͂.̶̰̀͝.̸̨̂͝",
            "Я̸̬͑͂ ̵̤̝͎̫̣͑̿̂н̷̱́̇̈͂е̵̩͔͎̳͗̏ ̷̱̃м̷̲̘͕̮͛̓о̸̖̉̋̔͠г̴̝̜̽̽́͘у̵̩͖͕̞͆͑ ̵̹̫̼̘͌͊̐̌н̷̞͔̔̾̈̚͜а̶̡̨͉̳̣͗̀̋̈́̈́й̸̭̯̟̺̾̋ͅт̷̳̪͚̓͜ӥ̸̜ ̸̡̬̟̃͊͊͘в̶̮͍̺̳͍̂̽͋͑̿ы̶̛̘̭͓̖̿̑̾̈х̴̗͇͒͝о̵̯̪̦̫̓͋́д̵̩̎͐͋̒͝.̵͈͓̤̍͂͝.̶̡̻͓̓.̵̤̞̉̄̋̿͊ ̶̳̙̒̈͘,",
            "П̸̝͙̋ӧ̷̰́ж̵͚̀̅а̸͔̅͌л̵̘̈́͑͜ӱ̸̜́й̸̫͍̓с̷̜̿̉т̶̛̰̝а̶̥͛.̶̜̀.̴͕̐̄.̴̭͇͗̚ ̵̼̱͑̅е̴̝͈͋͝с̸͈̓͋л̵͓̬͋и̷͓͆̆͜ ̴̧̪̉к̵̨͉͋т̵͕̾̎о̷͖̯͑-̸͍͊т̵̢͓̈́̇о̸̥̤̀ ̷̱̂̅с̸̯̈̂͜л̵̰͇̏ы̸̣̀ш̷̫͌͊ͅи̴̤͍̃т̴̦̼͂ ̷̹̲͗̈э̵̱͉̽̎т̴͕͍͘о̵͖̬̀.̸̤͂͝.̸̺̹̓.̷̪̩̅̚ ̸͓̈́̏п̶̰͉̀о̵̦̪́м̶̖̦̇̈о̵̱̭̾г̵̭̤̂̚и̷̖͎̂͛т̶̣̀̃е̷̨͔̓.̵̦͋.̷̛͚.̸̰͈̊͛"
        };

        private const int CreepyMessageChance = 20;
        private int _internalTurnCounter = 0;
        private string _internalActionString = "▓▓▓▓ бьет .... и .? .⸮";
        public GlitchHorrorAnomaly()
        {
            Name = "…---…";
            Description = "";
        }

        public override void ApplyEffect(Duel duel)
        {
            ApplyHorrorEffects(duel);
        }

        private void ApplyHorrorEffects(Duel duel)
        {
            duel.OnDamageCalculated += (attacker, defender, damage) =>
            {
                if (_internalTurnCounter == 13)
                {
                    duel.DuelMember1.HP = 666;
                    duel.DuelMember2.HP = 666;
                    duel.LastActionString = "П̴͙͉̆▓̷͚̂͝ж̴̡̟̏̑а̶̗͚͛̔л̸̠͈̂у̵͖̼̚й̸̲̲̌с̴̡̗̂̂т̴͉̞͒͝ӓ̶͇̺́͆,̷̭̓ͅ ̸̡͎̈́͂я̶̝̃ ̵̰̇̽у̴̠̐͌͜▓̷͕̠̏л̷̩̥́̽я̸̪̑́ю̷̩̭͊̇!̸̤̻̓ ̷̫̦͑Я̵͉͒ ̴̖͕͐̀▒̵̙̤̀е̶̰̽ ̶͖͎̔̽х̸̬̬̀о̵̘̽ч̵͇͆̈ӱ̸̺̤́͝ ̷̗͔́!̶̘̗̉́ ̴̞͛Я̸̡̪͆ ̵͈̥̿░̴͖͊̀ѐ̶̧̹̀ ̵͍̀̕▓̴̙͚̅▓̵̗̈ͅ▓̸̛̬̓▓̴̮͖̈́!̶̳̓";
                    duel.IsDuelEndedPrematurely = true;
                }

                if (_internalTurnCounter == 6 || _internalTurnCounter % 2 == 0 && new SafeRandom().Next(101) < CreepyMessageChance)
                {
                    string creepyMessage = _creepyMessages[new SafeRandom().Next(_creepyMessages.Length)];
                    duel.LastActionString = $"{creepyMessage}";
                }

                _internalActionString = ReplaceRandomCharactersToGlitch(_internalActionString);
                duel.LastActionString = _internalActionString;
            };

            duel.OnTurnEnded += (attacker, defender) =>
            {
                _internalTurnCounter++;

                duel.DuelMember1.SetCustomName(ReplaceRandomCharactersToGlitch(duel.DuelMember1.Name));
                duel.DuelMember2.SetCustomName(ReplaceRandomCharactersToGlitch(duel.DuelMember2.Name));

                duel.DuelMember1.HP = new SafeRandom().Next(-666, 667);
                duel.DuelMember2.HP = new SafeRandom().Next(-666, 667);
            };
        }

        private string ReplaceRandomCharactersToGlitch(string originalName)
        {
            var random = new SafeRandom();
            var nameChars = originalName.ToCharArray();

            if (nameChars.Length > 0)
            {
                int index = random.Next(nameChars.Length);
                nameChars[index] = _glitchSymbols[random.Next(_glitchSymbols.Length)][0];
            }

            return new string(nameChars);
        }
    }
}