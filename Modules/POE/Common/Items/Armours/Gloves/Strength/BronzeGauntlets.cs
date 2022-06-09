using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class BronzeGauntlets : Armour
    {
        public BronzeGauntlets()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Bronze Gauntlets";
            StatsRequirements.Str = 36;
            LevelRequired = 23;
            ArmourRating = random.Next(77, 97);
            Image = "Images/POE/Armour/Gloves/Strength/Bronze Gauntlets.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}