using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    public class CalculatorEnitity
    {
        public RPKShipher RPKShipher { get; set; }
        public string WorkType { get; set; }
        public string WorkName { get; set; }
        public string LegacyParam { get; set; }
        public int Priority { get; set; }
    }
}
