using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class TaxingGauntlets : Armour
    {
        public TaxingGauntlets()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Taxing Gauntlets";
            StatsRequirements.Str = 18;
            LevelRequired = 10;
            ArmourRating = random.Next(35, 42);
            Image = "Images/POE/Armour/Gloves/Strength/Taxing Gauntlets.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}