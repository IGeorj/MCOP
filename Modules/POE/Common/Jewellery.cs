using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Modules.POE.Common
{
    [Serializable]
    public class Jewellery : ItemBase
    {
        public JewelleryType Type { get; set; }
    }

    public enum JewelleryType
    {
        Amulet,
        Belt,
        Ring
    }
}
