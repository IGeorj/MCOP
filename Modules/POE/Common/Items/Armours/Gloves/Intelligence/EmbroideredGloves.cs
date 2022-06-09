using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class EmbroideredGloves : Armour
    {
        public EmbroideredGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Embroidered Gloves";
            StatsRequirements.Int = 54;
            LevelRequired = 36;
            EnergyShield = random.Next(25, 29);
            Image = "Images/POE/Armour/Gloves/Strength/Embroidered Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}