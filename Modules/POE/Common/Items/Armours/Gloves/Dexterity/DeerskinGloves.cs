using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class DeerskinGloves : Armour
    {
        public DeerskinGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Deerskin Gloves";
            StatsRequirements.Dex = 33;
            LevelRequired = 21;
            EvasionRating = random.Next(71, 89);
            Image = "Images/POE/Armour/Gloves/Strength/Deerskin Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}