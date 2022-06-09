using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class ConjurerGloves : Armour
    {
        public ConjurerGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Conjurer Gloves";
            StatsRequirements.Int = 79;
            LevelRequired = 55;
            EnergyShield = random.Next(37, 45);
            // Same Image
            Image = "Images/POE/Armour/Gloves/Strength/Embroidered Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}