using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class ArcanistGloves : Armour
    {
        public ArcanistGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Arcanist Gloves";
            StatsRequirements.Int = 95;
            LevelRequired = 60;
            EnergyShield = random.Next(45, 53);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Silk Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}