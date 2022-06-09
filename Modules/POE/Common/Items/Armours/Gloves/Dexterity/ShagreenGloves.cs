using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class ShagreenGloves : Armour
    {
        public ShagreenGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Shagreen Gloves";
            StatsRequirements.Dex = 78;
            LevelRequired = 54;
            EvasionRating = random.Next(177, 213);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Nubuck Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}