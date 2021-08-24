using Autodesk.Revit.DB;

namespace MathCalcPrice.Entity
{
    public class AnnexElement
    {
        public RPKShipher RPKShipher { get; set; }
        public WorkingPeriod WorkingPeriod { get; set; }
        public ConstructionPhase Phase { get; set; }
        public GroupingOfWork GroupOfWork { get; set; }
        public WorkType WorkType { get; set; }
        public ClassMaterial ClassMaterial { get; set; }
        public CalculatorEnitity CalculatorEnitity { get; set; }
        public Element Element { get; set; }
    }
}
