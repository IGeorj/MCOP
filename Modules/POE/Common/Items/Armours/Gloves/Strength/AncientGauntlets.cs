using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class AncientGauntlets : Armour
    {
        public AncientGauntlets()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Ancient Gauntlets";
            StatsRequirements.Str = 68;
            LevelRequired = 47;
            ArmourRating = random.Next(154, 174);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Bronze Gauntlets.png"; 
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}