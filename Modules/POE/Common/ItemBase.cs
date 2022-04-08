using MCOP.Services;

namespace MCOP.Modules.POE.Common
{

    [Serializable]
    public class ItemBase : Item
    {
        public string Name { get; set; } = "No Name";
        public string Image { get; set; } = "";
        public StatsRequirements? StatsRequirements { get; set; }
        public Sockets? Sockets { get; set; }
    }

    [Serializable]
    public class Item
    {
        public int Quality { get; set; } = 0;
        public int ItemLevel { get; set; } = 0;
        public int LevelRequired { get; set; } = 0;
        public bool IsCorrupted { get; set; } = false;
        public ItemType ItemType { get; set; } = ItemType.Normal;

        public SKTextLine QualityToText()
        {
            return ToTextLaneBase("Quality: ", Quality.ToString());
        }

        public SKTextLine ItemLevelToText()
        {
            return ToTextLaneBase("Item level: ", ItemLevel.ToString(), "#827A6C");
        }

        public SKTextLine? CorruptedToText()
        {
            return IsCorrupted ? new SKTextLine { new SKText("Corrupted", "#C50003") } : null;
        }

        private SKTextLine ToTextLaneBase(string text, string value, string valueColor = "#8787FE")
        {
            var light = "#827A6C";
            return new SKTextLine
            {
                new SKText(text, light),
                new SKText(value, valueColor)
            };
        }
    }

    [Serializable]
    public class StatsRequirements
    {
        public int Str { get; set; } = 0;
        public int Dex { get; set; } = 0;
        public int Int { get; set; } = 0;

        public SKTextLine StrToText()
        {
            return ToTextLaneBase("Str", Str.ToString());
        }

        public SKTextLine DexToText()
        {
            return ToTextLaneBase("Dex", Dex.ToString());
        }

        public SKTextLine IntToText()
        {
            return ToTextLaneBase("Int", Int.ToString());
        }

        private SKTextLine ToTextLaneBase(string text, string value)
        {
            var white = "#FFFFFF";
            var light = "#827A6C";
            return new SKTextLine
            {
                new SKText(value, white),
                new SKText(text, light)
            };
        }
    }

    [Serializable]
    public class Sockets
    {
        public int CountCurrent { get; set; } = 1;
        public int CountMax { get; set; } = 1;
        public int Linked { get; set; } = 1;
        public List<SocketColor> Colors { get; set; } = new List<SocketColor>();

    }

    public enum SocketColor
    {
        Red,
        Green,
        Blue,
        White,
        Black
    }

    public enum ItemType
    {
        Normal,
        Magic,
        Rare,
        Unique
    }
}
