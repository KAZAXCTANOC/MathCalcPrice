using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    public class GroupingOfWork
    {
        public RPKShipher RPKShipher { get; set; }
        public string GroupingRSO { get; set; }
        public string GroupingKP { get; set; }
        public int Priority { get; set; }
        public int Number { get; set; }
    }
}
