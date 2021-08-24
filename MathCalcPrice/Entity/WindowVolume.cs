using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    class Stvorka
    {
        public bool Main { get; set; } = false;
        public bool Up { get; set; } = false;
        public bool Down { get; set; } = false;
    }

    class StvorkaDataODB
    {
        public double[] Widths { get; set; } = new double[5];

        public double FramugaUp { get; set; } = 0.0;
        public double FramugaDown { get; set; } = 0.0;
        public double StvorkaHeight { get; set; } = 0.0;

        public StvorkaDataODB(Element elem)
        {
            for (int i = 1; i < 6; i++)
            {
                Widths[i - 1] = WindowVolume.GetData<double>(elem, $"Створка_{i}_Ширина") * Consts.FootToMillimeters;
            }
            FramugaUp = WindowVolume.GetData<double>(elem, "Фрамуга_верх_высота") * Consts.FootToMillimeters;
            FramugaDown = WindowVolume.GetData<double>(elem, "Фрамуга_низ_высота") * Consts.FootToMillimeters;
            StvorkaHeight = WindowVolume.GetData<double>(elem, "Створка_Гл_Высота") * Consts.FootToMillimeters;
        }
    }
    class StvorkaDataOK
    {
        public double[] Widths { get; set; } = new double[5];

        public double FramugaUp { get; set; } = 0.0;
        public double FramugaDown { get; set; } = 0.0;
        public double StvorkaHeight { get; set; } = 0.0;

        public StvorkaDataOK(Element elem)
        {
            for (int i = 1; i < 6; i++)
            {
                Widths[i - 1] = WindowVolume.GetData<double>(elem, $"#Створка_Ширина_{i}") * Consts.FootToMillimeters;
            }
            FramugaUp = WindowVolume.GetData<double>(elem, "#Фрамуга_верх_высота") * Consts.FootToMillimeters;
            FramugaDown = WindowVolume.GetData<double>(elem, "#Фрамуга_низ_высота") * Consts.FootToMillimeters;
            StvorkaHeight = WindowVolume.GetData<double>(elem, "#Створка_Гл_Высота") * Consts.FootToMillimeters;
        }
    }

    public class WindowsVolumeResult
    {
        public LinkFile Doc { get; set; }
        public double SquareOpenODB { get; set; }
        public double SquareCloseODB { get; set; }
        public double SquareOpenOK { get; set; }
        public double SquareCloseOK { get; set; }

        public double SquareOpen { get => (SquareOpenODB + SquareOpenOK) * Consts.Mm2ToM2; }
        public double SquareClose { get => (SquareCloseODB + SquareCloseOK) * Consts.Mm2ToM2; }

        public double PriceOpen { get => SquareOpen * 19800; }

        public double PriceClose { get => SquareClose * 7300; }

        public double Price { get => PriceOpen + PriceClose; }

        public double SquareSummary { get; set; }

        public override string ToString()
        {
            return $"[{Doc.Name}]\n" +
                $"* Площади:\n" +
                $"\t[одб]\n" +
                $"\t    Общая = {(SquareOpenODB + SquareCloseODB) * Consts.Mm2ToM2} м2\n" +
                $"\t   Глухие = {SquareCloseODB * Consts.Mm2ToM2} м2\n" +
                $"\t Открытые = {SquareOpenODB * Consts.Mm2ToM2} м2\n" +
                $"\t[ок]\n" +
                $"\t    Общая = {(SquareOpenOK + SquareCloseOK) * Consts.Mm2ToM2} м2\n" +
                $"\t   Глухие = {SquareCloseOK * Consts.Mm2ToM2} м2\n" +
                $"\t Открытые = {SquareOpenOK * Consts.Mm2ToM2} м2\n" +
                $"* Стоимость:\n" +
                $"\t   Общая = {Price.ToString("N1", CultureInfo.CurrentCulture)}\n" +
                $"\t  Глухие = {PriceClose.ToString("N1", CultureInfo.CurrentCulture)}\n" +
                $"\tОткрытые = {PriceOpen.ToString("N1", CultureInfo.CurrentCulture)}";
        }
    }

    public class WindowVolume
    {
        private ElementCategoryFilter _filter = new ElementCategoryFilter(BuiltInCategory.OST_Windows);
        private bool _useNewTypeOfCalculations_OK_WINDOWS;

        public WindowVolume(bool useNewTypeOfCalculations_OK_WINDOWS = false)
        {
            _useNewTypeOfCalculations_OK_WINDOWS = useNewTypeOfCalculations_OK_WINDOWS;
        }

        private List<Element> GetDependent(Element element)
        {
            List<Element> elems = new List<Element>();
            foreach (var rebarid in element.GetDependentElements(_filter))
            {
                elems.Add(element.Document.GetElement(rebarid));
            }
            return elems;
        }

        public static T GetData<T>(Element elem, string str) where T : IConvertible
        {
            var p = elem.LookupParameter(str);
            if (typeof(T) == typeof(int))
            {
                if (p != default)
                    return (T)Convert.ChangeType(p.AsInteger(), typeof(T));
            }
            if (typeof(T) == typeof(string))
            {
                if (p != default)
                    return (T)Convert.ChangeType(p.AsString(), typeof(T));
            }
            if (typeof(T) == typeof(double))
            {
                if (p != default)
                    return (T)Convert.ChangeType(p.AsDouble(), typeof(T));
            }
            return default;
        }
        private static int GetDataInt(Element elem, string str)
        {
            var p = elem.LookupParameter(str);
            if (p != default)
                return p.AsInteger();
            return -1;
        }

        public WindowsVolumeResult Get(List<AnnexElement> elements)
        {
            var windows = elements
                .Where(x => (BuiltInCategory)x.Element.Category.Id.IntegerValue == BuiltInCategory.OST_Windows)
                .ToList();


            WindowsVolumeResult r = new WindowsVolumeResult();
            foreach (var window in elements)
            {
                var fullness = AllClassificatorsFull(window.Element);
                var type = window.Element.Document.GetElement(window.Element.GetTypeId());
                r.SquareSummary += GetSquare(type);
            }
            r.SquareSummary /= 1000000;
            return r;
        }

        private double GetSquare(Element window)
        {
            double width = GetData<double>(window, "ADSK_Размер_Ширина") * Consts.FootToMillimeters;
            double height = GetData<double>(window, "ADSK_Размер_Высота") * Consts.FootToMillimeters;
            return width * height;
        }

        private int AllClassificatorsFull(Element window)
        {
            var c = GetData<string>(window, Consts.ClassConstruction);
            var m = GetData<string>(window, Consts.ClassMaterial);
            var p = GetData<string>(window, Consts.ClassParams);
            var xm = GetData<string>(window, Consts.ClassChars);
            var s = GetData<string>(window, Consts.ClassSection);
            var f = GetData<string>(window, Consts.ClassFloor);
            if (c != default && m != default && p != default && s != default && f != default && xm != default)
            {
                if (c == "ОДБ") return 1;
                else if (c == "ОК") return 0;
            }
            return -1;
        }

        private double GetSquaresOpen(List<Element> windows)
        {
            double square = 0.0;
            foreach (var window in windows)
            {
                Element type = window.Document.GetElement(window.GetTypeId());
                square += GetSquareInner(window);
            }
            return square;
        }

        private double GetSquareInner(Element window)
        {
            var dverotrk = window.LookupParameter("ДверьОткрывание");
            double square = 0.0;
            if (dverotrk != default)
            {
                if (dverotrk.AsInteger() > 0)
                {
                    double width = GetData<double>(window, "ADSK_Размер_Ширина") * Consts.FootToMillimeters;
                    double height = GetData<double>(window, "ADSK_Размер_Высота") * Consts.FootToMillimeters;
                    square += width * height;
                }
            }
            else
            {
                Stvorka[] opening = new Stvorka[5];
                StvorkaDataODB sd = new StvorkaDataODB(window);
                for (int i = 1; i < 6; i++)
                {
                    opening[i - 1] = new Stvorka
                    {
                        Main = GetData<int>(window, $"#Открывание_индекс_{i}_створки") > 0,
                        Up = GetData<int>(window, $"#Открывание_индекс_{i}'_створки") > 0,
                        Down = GetData<int>(window, $"#Открывание_индекс_{i}''_створки") > 0
                    };
                }
                for (int i = 0; i < opening.Length; i++)
                {
                    square += GetOpen(opening[i], i, sd);
                }
            }
            return square;
        }

        private double GetSquareOKCLosed(Element window)
        {
            double square = 0.0;
            Stvorka[] opening = new Stvorka[5];
            StvorkaDataOK sd = new StvorkaDataOK(window);
            for (int i = 1; i < 6; i++)
            {
                opening[i - 1] = new Stvorka
                {
                    Main = GetDataInt(window, $"#Открывание_Створки_{i}") == 0,
                    Up = GetDataInt(window, $"#Открывание_Створки_{i}'") == 0,
                    Down = GetDataInt(window, $"#Открывание_Створки_{i}''") == 0
                };
            }
            for (int i = 0; i < opening.Length; i++)
            {
                square += GetOpen(opening[i], i, sd);
            }
            return square;
        }

        private double GetSquareInnerType(Element window)
        {
            double square = 0.0;
            Stvorka[] opening = new Stvorka[5];
            StvorkaDataOK sd = new StvorkaDataOK(window);
            for (int i = 1; i < 6; i++)
            {
                opening[i - 1] = new Stvorka
                {
                    Main = GetData<int>(window, $"#Открывание_Створки_{i}") > 0,
                    Up = GetData<int>(window, $"#Открывание_Створки_{i}'") > 0,
                    Down = GetData<int>(window, $"#Открывание_Створки_{i}''") > 0
                };
            }
            for (int i = 0; i < opening.Length; i++)
            {
                square += GetOpen(opening[i], i, sd);
            }
            return square;
        }

        private double GetOpen(Stvorka item, int i, StvorkaDataODB sd)
        {
            double val = 0.0;
            if (item.Main) val += sd.Widths[i] * sd.StvorkaHeight;
            if (item.Down) val += sd.Widths[i] * sd.FramugaDown;
            if (item.Up) val += sd.Widths[i] * sd.FramugaUp;
            return val;
        }
        private double GetOpen(Stvorka item, int i, StvorkaDataOK sd)
        {
            double val = 0.0;
            if (item.Main) val += sd.Widths[i] * sd.StvorkaHeight;
            if (item.Down) val += sd.Widths[i] * sd.FramugaDown;
            if (item.Up) val += sd.Widths[i] * sd.FramugaUp;
            return val;
        }
    }
}
