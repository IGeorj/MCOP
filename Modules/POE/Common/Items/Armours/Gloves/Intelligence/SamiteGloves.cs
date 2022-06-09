using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class SamiteGloves : Armour
    {
        public SamiteGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Samite Gloves";
            StatsRequirements.Int = 68;
            LevelRequired = 47;
            EnergyShield = random.Next(32, 38);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Silk Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}