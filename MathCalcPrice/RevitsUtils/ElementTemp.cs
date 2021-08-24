using Autodesk.Revit.DB;
using MathCalcPrice.Entity;

namespace MathCalcPrice.RevitsUtils
{
    public class ElementTemp
    {
        public string Name { get; set; }
        public int ElementId { get; set; }
        public RPKShipher Code { get; set; }
        public Element Element { get; set; }
        public bool Valid { get; set; }
        public string NonValidInfo { get; set; }
    }
}
