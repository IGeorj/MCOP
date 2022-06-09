using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class SilkGloves : Armour
    {
        public SilkGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Silk Gloves";
            StatsRequirements.Int = 39;
            LevelRequired = 25;
            EnergyShield = random.Next(18, 24);
            Image = "Images/POE/Armour/Gloves/Strength/Silk Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}