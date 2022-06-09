using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class SteelGauntlets : Armour
    {
        public SteelGauntlets()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Steel Gauntlets";
            StatsRequirements.Str = 52;
            LevelRequired = 35;
            ArmourRating = random.Next(116, 127);
            Image = "Images/POE/Armour/Gloves/Strength/Steel Gauntlets.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}