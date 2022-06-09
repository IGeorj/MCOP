using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class GoathideGloves : Armour
    {
        public GoathideGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Goathide Gloves";
            StatsRequirements.Dex = 17;
            LevelRequired = 9;
            EvasionRating = random.Next(32, 43);
            Image = "Images/POE/Armour/Gloves/Strength/Goathide Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}