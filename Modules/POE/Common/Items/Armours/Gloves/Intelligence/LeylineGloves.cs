using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class LeylineGloves : Armour
    {
        public LeylineGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Leyline Gloves";
            StatsRequirements.Int = 18;
            LevelRequired = 10;
            EnergyShield = random.Next(9, 11);
            Image = "Images/POE/Armour/Gloves/Strength/Leyline Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}