using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class SlinkGloves : Armour
    {
        public SlinkGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Slink Gloves";
            StatsRequirements.Dex = 95;
            LevelRequired = 70;
            EvasionRating = random.Next(242, 279);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Nubuck Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}