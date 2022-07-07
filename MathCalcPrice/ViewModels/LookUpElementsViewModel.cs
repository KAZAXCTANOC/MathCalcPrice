using MathCalcPrice.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.ViewModels
{
    class LookUpElementsViewModel
    {
        public List<Groups> Groups { get; set; }
        public List<GroupWithCount> groupWithCounts { get; set; }
        public LookUpElementsViewModel()
        {

        }
    }
}
