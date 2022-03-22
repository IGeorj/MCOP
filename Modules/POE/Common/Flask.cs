using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Modules.POE.Common
{
    [Serializable]
    public class Flask : ItemBase
    {
        public decimal Duration { get; set; }
        public int Usage { get; set; }
        public int Capacity { get; set; }
    }
}
