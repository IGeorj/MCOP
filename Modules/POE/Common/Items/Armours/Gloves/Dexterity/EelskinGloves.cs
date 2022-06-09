using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class EelskinGloves : Armour
    {
        public EelskinGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Eelskin Gloves";
            StatsRequirements.Dex = 56;
            LevelRequired = 38;
            EvasionRating = random.Next(125, 148);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Goathide Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}