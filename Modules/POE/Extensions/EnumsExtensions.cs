using MCOP.Modules.POE.Common;

namespace MCOP.Modules.POE.Extensions
{
    public static class EnumsExtensions
    {

        public static int GetMaxSocketCount(this ArmourType type)
        {
            switch (type)
            {
                case ArmourType.Boots:
                case ArmourType.Gloves:
                case ArmourType.Helmet:
                    return 4;
                case ArmourType.Shield:
                    return 3;
                case ArmourType.BodyArmour:
                    return 6;
                default:
                    return 1;
            }
        }
    }
}
