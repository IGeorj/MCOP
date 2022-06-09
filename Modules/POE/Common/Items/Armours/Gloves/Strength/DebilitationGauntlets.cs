using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class DebilitationGauntlets : Armour
    {
        public DebilitationGauntlets()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Debilitation Gauntlets";
            StatsRequirements.Str = 101;
            LevelRequired = 70;
            ArmourRating = random.Next(236, 272);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Taxing Gauntlets.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}