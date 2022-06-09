using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class WoolGloves : Armour
    {
        public WoolGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Wool Gloves";
            StatsRequirements.Int = 9;
            LevelRequired = 3;
            EnergyShield = random.Next(5, 8);
            Image = "Images/POE/Armour/Gloves/Strength/Wool Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}