using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class GaucheGloves : Armour
    {
        public GaucheGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Gauche Gloves";
            StatsRequirements.Dex = 18;
            LevelRequired = 10;
            EvasionRating = random.Next(35, 42);
            Image = "Images/POE/Armour/Gloves/Strength/Gauche Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}