using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class StealthGloves : Armour
    {
        public StealthGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Stealth Gloves";
            StatsRequirements.Dex = 97;
            LevelRequired = 62;
            EvasionRating = random.Next(231, 266);
            // Same Image 
            Image = "Images/POE/Armour/Gloves/Strength/Deerskin Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}