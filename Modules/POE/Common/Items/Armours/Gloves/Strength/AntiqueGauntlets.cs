using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class AntiqueGauntlets : Armour
    {
        public AntiqueGauntlets()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Antique Gauntlets";
            StatsRequirements.Str = 58;
            LevelRequired = 39;
            ArmourRating = random.Next(129, 155);
            Image = "Images/POE/Armour/Gloves/Strength/Antique Gauntlets.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}