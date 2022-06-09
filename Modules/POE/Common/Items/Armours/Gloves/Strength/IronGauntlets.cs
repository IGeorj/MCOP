using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class IronGauntlets : Armour
    {
        public IronGauntlets()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Iron Gauntlets";
            StatsRequirements.Str = 6;
            ArmourRating = random.Next(6, 10);
            Image = "Images/POE/Armour/Gloves/Strength/Iron Gauntlets.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}