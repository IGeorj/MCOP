using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class SharkskinGloves : Armour
    {
        public SharkskinGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Sharkskin Gloves";
            StatsRequirements.Dex = 66;
            LevelRequired = 45;
            EvasionRating = random.Next(148, 163);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Deerskin Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}