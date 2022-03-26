using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    public class SavedData
    {
        public string WorkName { get; set; }
        public double TotalCostWork { get; set; } = 0;
        public double TotalCostMaterial { get; set; } = 0;
        public double TotalVolume { get; set; } = 0;
    }
}
