using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class VelvetGloves : Armour
    {
        public VelvetGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Velvet Gloves";
            StatsRequirements.Int = 21;
            LevelRequired = 12;
            EnergyShield = random.Next(10, 14);
            Image = "Images/POE/Armour/Gloves/Strength/Velvet Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}