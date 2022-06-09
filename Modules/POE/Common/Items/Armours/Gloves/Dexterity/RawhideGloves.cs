using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCOP.Common;

namespace MCOP.Modules.POE.Common.Items.Armours.Gloves
{
    public class RawhideGloves : Armour
    {
        public RawhideGloves()
        {
            SecureRandom random = new SecureRandom();
            StatsRequirements = new StatsRequirements();
            Name = "Rawhide Gloves";
            StatsRequirements.Dex = 9;
            LevelRequired = 3;
            EvasionRating = random.Next(13, 19);
            Image = "Images/POE/Armour/Gloves/Strength/Rawhide Gloves.png";
            ArmorType = ArmourTypeEnum.Gloves;
        }
    }
}