using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class GruellingGauntlets : Armour
    {
        public GruellingGauntlets()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Gruelling Gauntlets";
            StatsRequirements.Str = 59;
            LevelRequired = 40;
            ArmourRating = random.Next(132, 153);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Taxing Gauntlets.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}