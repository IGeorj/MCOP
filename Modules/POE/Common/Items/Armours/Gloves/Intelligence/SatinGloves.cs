using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class SatinGloves : Armour
    {
        public SatinGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Satin Gloves";
            StatsRequirements.Int = 60;
            LevelRequired = 41;
            EnergyShield = random.Next(28, 34);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Velvet Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}