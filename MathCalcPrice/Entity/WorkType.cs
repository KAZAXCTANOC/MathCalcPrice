using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    public class WorkType
    {
        public RPKShipher RPKShipher { get; set; }
        public string TypeRSOKP { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Unit { get; set; }
        public string ShortName { get; set; }
        public string Accrual { get; set; }
        public int Order { get; set; }
    }
}
