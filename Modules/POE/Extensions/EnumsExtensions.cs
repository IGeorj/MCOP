using MCOP.Modules.POE.Common;

namespace MCOP.Modules.POE.Extensions
{
    public static class EnumsExtensions
    {

        public static int GetMaxSocketCount(this ArmourTypeEnum type)
        {
            return type switch
            {
                ArmourTypeEnum.Boots or ArmourTypeEnum.Gloves or ArmourTypeEnum.Helmet => 4,
                ArmourTypeEnum.Shield => 3,
                ArmourTypeEnum.BodyArmour => 6,
                _ => 1,
            };
        }
    }
}
