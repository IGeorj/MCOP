using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Modules.POE.Common
{
    [Serializable]
    public class Armour : ItemBase
    {
        public int ArmourRating { get; set; }
        public int EvasionRating { get; set; }
        public int EnergyShield { get; set; }
        public ArmourType Type { get; set; }
    }

    public enum ArmourType
    {
        BodyArmour,
        Boots,
        Gloves,
        Helmet,
        Shield
    }
}
