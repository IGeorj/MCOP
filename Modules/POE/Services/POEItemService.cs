using MCOP.Modules.POE.Common;
using MCOP.Modules.POE.Extensions;
using MCOP.Services;

namespace MCOP.Modules.POE.Services
{
    public class POEItemService : IBotService
    {
        public Armour CreateArmour(int armour, int evasion, int energy, ArmourType type)
        {
            var stats = new StatsRequirements();
            var sockets = new Sockets();
            sockets.Colors.Add(SocketColor.White);
            sockets.CountMax = type.GetMaxSocketCount();

            return new Armour
            {
                Type = type,
                EnergyShield = energy,
                ArmourRating = armour,
                EvasionRating = evasion,
                ItemType = ItemType.Normal,
                Quality = 0,
                IsCorrupted = false,
                ItemLevel = 0,
                LevelRequired = 0,
                Image = "Images/POE/Simple_Robe.png",
                Sockets = sockets,
                StatsRequirements = stats
            };
        }
    }
}
