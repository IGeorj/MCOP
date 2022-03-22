using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Modules.POE.Common
{
    [Serializable]
    public class ItemBase : Item
    {
        public string Image { get; set; } = "";
        public StatsRequirements? StatsRequirements { get; set; }
        public Sockets? Sockets { get; set; }
    }

    [Serializable]
    public class Item
    {
        public int Quality { get; set; }
        public int ItemLevel { get; set; }
        public int LevelRequired { get; set; }
        public bool IsCorrupted { get; set; }
        public ItemType ItemType { get; set; } = ItemType.Normal;

    }

    [Serializable]
    public class StatsRequirements
    {
        public int Int { get; set; } = 0;
        public int Str { get; set; } = 0;
        public int Agl { get; set; } = 0;
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
