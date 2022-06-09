using MCOP.Services;
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
        public int? ArmourRating { get; set; }
        public int? EvasionRating { get; set; }
        public int? EnergyShield { get; set; }
        public ArmourTypeEnum ArmorType { get; set; } = ArmourTypeEnum.BodyArmour;

        public SKTextLine? ArmourRatingToText()
        {
            if (ArmourRating.HasValue == false)
            {
                return null;
            }

            return new SKTextLine()
            {
                new SKText("armour: ", TextColors.LightHexColor),
                new SKText(ArmourRating.Value.ToString(), TextColors.BlueHexColor)
            };
        }
        public SKTextLine? EvasionRatingToText()
        {
            if (EvasionRating.HasValue == false)
            {
                return null;
            }

            return new SKTextLine()
            {
                new SKText("evasion: ", TextColors.LightHexColor),
                new SKText(EvasionRating.Value.ToString(), TextColors.BlueHexColor)
            };
        }
        public SKTextLine? EnergyShieldToText()
        {
            if (EnergyShield.HasValue == false)
            {
                return null;
            }

            return new SKTextLine()
            {
                new SKText("energy shield: ", TextColors.LightHexColor),
                new SKText(EnergyShield.Value.ToString(), TextColors.BlueHexColor)
            };
        }
    }

    public enum ArmourTypeEnum
    {
        BodyArmour,
        Boots,
        Gloves,
        Helmet,
        Shield
    }
}
