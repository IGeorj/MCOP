using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class TitanGauntlets : Armour
    {
        public TitanGauntlets()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Titan Gauntlets";
            StatsRequirements.Str = 98;
            LevelRequired = 69;
            ArmourRating = random.Next(242, 279);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Steel Gauntlets.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}