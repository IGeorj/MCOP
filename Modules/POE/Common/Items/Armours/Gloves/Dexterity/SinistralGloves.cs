using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class SinistralGloves : Armour
    {
        public SinistralGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Sinistral Gloves";
            StatsRequirements.Dex = 101;
            LevelRequired = 70;
            EvasionRating = random.Next(236, 272);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Gauche Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}