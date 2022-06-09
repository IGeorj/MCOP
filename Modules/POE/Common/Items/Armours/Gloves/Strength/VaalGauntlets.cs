using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class VaalGauntlets : Armour
    {
        public VaalGauntlets()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Vaal Gauntlets";
            StatsRequirements.Str = 100;
            LevelRequired = 63;
            ArmourRating = random.Next(232, 266);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Bronze Gauntlets.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}