using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class SouthswingGloves : Armour
    {
        public SouthswingGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Southswing Gloves";
            StatsRequirements.Dex = 59;
            LevelRequired = 40;
            EvasionRating = random.Next(132, 153);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Gauche Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}