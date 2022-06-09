using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class NubuckGloves : Armour
    {
        public NubuckGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Nubuck Gloves";
            StatsRequirements.Dex = 50;
            LevelRequired = 33;
            EvasionRating = random.Next(109, 123);
            Image = "Images/POE/Armour/Gloves/Strength/Nubuck Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}