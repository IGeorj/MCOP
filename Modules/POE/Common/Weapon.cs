using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Modules.POE.Common
{
    [Serializable]
    public class Weapon : ItemBase
    {
        public int BaseDamage { get; set; }
        public int BaseCritChance { get; set; }
        public decimal AttackPerSecond { get; set; }
        public WeaponType Type { get; set; }
    }

    public enum WeaponType
    {
        Axe,
        Bow,
        Claw,
        Dagger,
        Mace,
        Sceptre,
        Staff,
        Sword,
        Wand
    }
}
