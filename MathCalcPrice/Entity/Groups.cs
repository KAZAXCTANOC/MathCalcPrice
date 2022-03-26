using System.Collections.Generic;

namespace MathCalcPrice.Entity
{
    public class Groups
    {
        public string GroupName { get; set; }
        public List<ParameterClassifiers> parameterClassifiers { get; set; } = new List<ParameterClassifiers>();
    }
}
