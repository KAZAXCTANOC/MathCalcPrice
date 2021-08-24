using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    class Consts
    {
        public const double FootToM3 = 0.028316846592;
        public const double FootToM2 = 0.09290304;
        public const double FootToMillimeters = 304.8;
        public const double Mm2ToM2 = 1.0e-6;

        public const string ClassParams = "Классификатор_Параметры";
        public const string ClassConstruction = "Классификатор_Конструкция";
        public const string ClassMaterial = "Классификатор_Материал";
        public const string ClassSection = "Классификатор_Секция";
        public const string ClassFloor = "Классификатор_Этаж";
        public const string ClassChars = "Классификатор_Характеристика_материала"; // материал


        private static double TwoSum(double a, double b, out double t)
        {
            var s = a + b;
            var bs_ = s - a;
            var as_ = s - bs_;
            t = (b - bs_) + (a - as_);
            return s;
        }
        public static double SumRump(IEnumerable<double> arr)
        {
            double s = 0.0, c = 0.0, e;
            foreach (double x in arr)
            {
                s = TwoSum(s, x, out e);
                c += e;
            }
            return s + c;
        }
    }
}
