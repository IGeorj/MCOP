using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class NexusGloves : Armour
    {
        public NexusGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Leyline Gloves";
            StatsRequirements.Int = 101;
            LevelRequired = 70;
            EnergyShield = random.Next(47, 55);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Leyline Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}