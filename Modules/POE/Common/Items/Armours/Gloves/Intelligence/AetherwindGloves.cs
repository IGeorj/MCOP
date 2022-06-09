using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class AetherwindGloves : Armour
    {
        public AetherwindGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Aetherwind Gloves";
            StatsRequirements.Int = 59;
            LevelRequired = 40;
            EnergyShield = random.Next(28, 32);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Leyline Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}