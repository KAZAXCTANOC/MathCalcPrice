using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    public class ClassMaterial
    {
        public RPKShipher RPKShipher { get; set; }
        public List<Material> Materials { get; set; }
        public double PriceForWork { get; set; }
        public double PriceForMaterial { get; set; }
        public string Units { get; set; }
        public override string ToString()
        {
            StringBuilder s = new StringBuilder($"Шифр: [{RPKShipher}], Цена за работу: {PriceForWork}");
            s.Append("\nМатериалы:\n");
            foreach (var m in Materials)
            {
                s.Append($"{m}\n");
            }
            return s.ToString();
        }
    }
}
