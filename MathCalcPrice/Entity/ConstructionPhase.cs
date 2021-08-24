using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    public class ConstructionPhase
    {
        public RPKShipher RPKShipher { get; set; }
        public string Phase { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
    }
}
