using MCOP.Modules.POE.Common.Enums;
using MCOP.Services;

namespace MCOP.Modules.POE.Common
{

    public static class TextColors
    {
        public static string LightHexColor { get; } = "#827A6C";
        public static string WhiteHexColor { get; } = "#FFFFFF";
        public static string BlueHexColor { get; } = "#8787FE";
        public static string CorruptedHexColor { get; } = "#C50003";
    }

    [Serializable]
    public class ItemBase : Item
    {
        public string Name { get; set; } = "Base Name";
        public string Image { get; set; } = "";
        public StatsRequirements? StatsRequirements { get; set; }
        public Sockets? Sockets { get; set; }

        public SKTextLine NameToText()
        {
            return new SKTextLine
            {
                new SKText(Name, TextColors.WhiteHexColor, 18.0f)
            };
        }

    }

    [Serializable]
    public class Item
    {
        public int? Quality { get; set; }
        public int ItemLevel { get; set; } = 0;
        public int LevelRequired { get; set; } = 0;
        public bool IsCorrupted { get; set; } = false;
        public ItemType ItemType { get; set; } = ItemType.Normal;

        public SKTextLine? QualityToText()
        {
            if (Quality.HasValue == false)
            {
                return null;
            }

            return new SKTextLine
            {
                new SKText("Quality: ", TextColors.LightHexColor),
                new SKText(Quality.Value.ToString() + "%", TextColors.BlueHexColor)
            };
        }

        public SKTextLine ItemLevelToText()
        {
            return new SKTextLine
            {
                new SKText("Item level: ", TextColors.LightHexColor),
                new SKText(ItemLevel.ToString(), TextColors.WhiteHexColor)
            };
        }

        public SKTextLine? CorruptedToText()
        {
            return IsCorrupted ? new SKTextLine { new SKText("Corrupted", TextColors.CorruptedHexColor) } : null;
        }
    }

    [Serializable]
    public class StatsRequirements
    {
        public int? Str { get; set; }
        public int? Dex { get; set; }
        public int? Int { get; set; }

        public SKTextLine StatsToTextLine()
        {
            SKTextLine textLine = new SKTextLine();
            List<SKText> textList = new List<SKText>();


            if (Str.HasValue)
            {
                textList.Add(new SKText(Str.Value.ToString(), TextColors.WhiteHexColor));
                textList.Add(new SKText((Dex.HasValue || Int.HasValue) ? " Str, " : " Str", TextColors.LightHexColor));
            }
            if (Dex.HasValue)
            {
                textList.Add(new SKText(Dex.Value.ToString(), TextColors.WhiteHexColor));
                textList.Add(new SKText(Int.HasValue ? " Dex, " : " Dex", TextColors.LightHexColor));
            }
            if (Int.HasValue)
            {
                textList.Add(new SKText(Int.Value.ToString(), TextColors.WhiteHexColor));
                textList.Add(new SKText(" Int", TextColors.LightHexColor));
            }

            textLine.Add(textList);

            return textLine;
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
}
