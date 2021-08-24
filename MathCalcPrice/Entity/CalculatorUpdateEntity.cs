using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    public class CalculatorUpdateEntity
    {
        public string WorkType { get; set; }
        public string WorkName { get; set; }
        public double Value { get; set; }
        public double Volume { get; set; }
        public double MaterialCost { get; set; }
    }
}
