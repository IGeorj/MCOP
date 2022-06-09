using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class SorcererGloves : Armour
    {
        public SorcererGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Sorcerer Gloves";
            StatsRequirements.Int = 97;
            LevelRequired = 69;
            EnergyShield = random.Next(49, 58);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Embroidered Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}