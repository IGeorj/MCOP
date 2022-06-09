using MCOP.Common;
using MCOP.Modules.POE.Common;
using MCOP.Modules.POE.Common.Enums;
using MCOP.Modules.POE.Extensions;
using MCOP.Services;

namespace MCOP.Modules.POE.Services
{
    public class POEItemService : IBotService
    {
        DirectoryInfo armorDInfo = new DirectoryInfo("Images/POE/Armour");
        public Armour CreateRandomArmour()
        {
            try
            {
                SecureRandom random = new SecureRandom();

                FileInfo[] images = armorDInfo.GetFiles("*.png", SearchOption.AllDirectories);
                FileInfo randomImg = images[random.Next(images.Length)];

                var stats = new StatsRequirements();
                stats.Int = random.Next(101);
                stats.Dex = random.Next(101);
                stats.Str = random.Next(101);

                var armour = new Armour();
                armour.ArmourRating = random.Next(101);
                armour.EvasionRating = random.Next(101);
                armour.EnergyShield = random.Next(101);
                armour.Image = randomImg.FullName;
                armour.Name = Path.GetFileNameWithoutExtension(randomImg.Name);
                armour.StatsRequirements = stats;
                armour.Quality = random.Next(21);
                armour.ItemLevel = random.Next(101);
                armour.ItemLevel = random.Next(101);
                armour.IsCorrupted = random.NextBool();
                armour.ItemType = random.ChooseRandomEnumValue<ItemType>();
                armour.ArmorType = random.ChooseRandomEnumValue<ArmourTypeEnum>();

                return armour;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
