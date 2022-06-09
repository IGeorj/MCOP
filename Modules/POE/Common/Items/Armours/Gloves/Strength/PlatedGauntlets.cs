using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class PlatedGauntlets : Armour
    {
        public PlatedGauntlets()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Plated Gauntlets";
            StatsRequirements.Str = 20;
            LevelRequired = 11;
            ArmourRating = random.Next(39, 51);
            Image = "Images/POE/Armour/Gloves/Strength/Plated Gauntlets.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}