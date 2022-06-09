using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class GoliathGauntlets : Armour
    {
        public GoliathGauntlets()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Goliath Gauntlets";
            StatsRequirements.Str = 77;
            LevelRequired = 53;
            ArmourRating = random.Next(174, 200);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Steel Gauntlets.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
        
    }
}