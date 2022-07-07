using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    public class GroupWithCount
    {
        public string Classifier { get; set; }
        public string GroupName { get; set; }
        public int Count { get { return Classifiers.Count(); } }
        public List<ParameterClassifiers> Classifiers { get; set; } = new List<ParameterClassifiers>();
    }
}
