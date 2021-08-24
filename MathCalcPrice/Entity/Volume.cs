using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    public enum VolumeInfoTypeEnum
    {
        Windows,
        AddedPms,
        Misc
    }
    public class VolumeInfo
    {
        public double Value { get; set; } = 0.0;
        public string Info { get; set; } = default;
        public VolumeInfoTypeEnum VolumeType { get; set; } = VolumeInfoTypeEnum.Misc;
        public dynamic Other { get; set; } = default;
    }

    public class Volume
    {
        public const string NonValid = "[NULL]";
        private StringBuilder _warnings = new StringBuilder();
        public string Warnings { get { return _warnings.ToString(); } }

        public double Dlina { get; set; } = 0.0;
        public double AddedPms { get; set; } = 0.0;

        public const int CountInRiser = 9;
        public const int Rounded = 5;



        private const string ADSK_V_pogonnih_metrax = "844a01e2-19fc-4dc5-baa0-a4bda30ef1f6";

        public double GetMassByDiametr(double diametr)
        {
            switch (Math.Round(diametr, 0))
            {
                case 3: return 0.056;
                case 4: return 0.0986;
                case 5: return 0.1541;
                case 6: return 0.222;
                case 8: return 0.395;
                case 10: return 0.617;
                case 12: return 0.888;
                case 14: return 1.21;
                case 16: return 1.58;
                case 18: return 2.0;
                case 20: return 2.47;
                case 22: return 2.98;
                case 25: return 3.85;
                case 28: return 4.38;
                case 32: return 6.31;
                default: return 0.0;
            }
        }

        private VolumeInfo CalcPieces(List<AnnexElement> elements) => new VolumeInfo { Value = elements.Count, Info = $"{elements.Count} шт." };

        private VolumeInfo CalcM2(List<AnnexElement> elements)
        {
            double P = 0;
            WindowVolume wv = new WindowVolume();
            if (elements.Count > 0 && elements[0].WorkType.RPKShipher.C == "ОК" || elements[0].WorkType.RPKShipher.C == "ОДБ")
                return new VolumeInfo { VolumeType = VolumeInfoTypeEnum.Windows, Other = wv.Get(elements) };

            foreach (var elem in elements)
            {
                if (elem.RPKShipher.M == "СА")
                {
                    var sq = elem.Element.LookupParameter("ADSK_Площадь на 1 стержень")?.AsDouble() * Consts.FootToMillimeters;
                    if (sq != null && sq.HasValue)
                    {
                        P += sq.Value;
                        continue;
                    }
                }

                if (elem.WorkType.RPKShipher.C == "ВДВ" || elem.WorkType.RPKShipher.C == "НДВ")
                {
                    ElementType elType = elem.Element.Document.GetElement(elem.Element.GetTypeId()) as ElementType;

                    var Shirina = elType.LookupParameter("Ширина")?.AsDouble() * Consts.FootToMillimeters;
                    var ADSK_Razmer_Vysota = elType.LookupParameter("ADSK_Размер_Высота")?.AsDouble() * Consts.FootToMillimeters;

                    if (Shirina != null & ADSK_Razmer_Vysota != null)
                    {
                        P += (Shirina.Value * ADSK_Razmer_Vysota.Value) / 1000000;
                    }

                    continue;
                }


                double? p = elem.Element.LookupParameter("Площадь")?.AsDouble() * Consts.FootToM2;
                if (p != null && p.HasValue)
                    P += p.Value;

            }
            P = Math.Round(P, Rounded);

            return new VolumeInfo { Value = P, Info = $"{P} м^2" };
        }

        private VolumeInfo CalcM3(List<AnnexElement> elements)
        {
            double S = 0;
            foreach (var elem in elements)
            {
                double? s = elem.Element.LookupParameter("Объем")?.AsDouble();
                if (s.HasValue)
                {
                    S += s.Value * Consts.FootToM3;
                }
            }

            S = Math.Round(S, Rounded);
            return new VolumeInfo { Value = S, Info = $"{S} м^3" };
        }

        private VolumeInfo CalcPM(List<AnnexElement> elements)
        {
            double PM = 0;

            foreach (var elem in elements)
            {
                if (elem.RPKShipher.C == "ЛСВ")
                {
                    var dlina = elem.Element.LookupParameter("Длина_лидер скважины")?.AsDouble() * Consts.FootToMillimeters;
                    if (dlina != null)
                        PM += dlina.Value;
                }
                else
                {
                    double? dlina = elem.Element.LookupParameter("ADSK_Размер_Длина")?.AsDouble() * Consts.FootToMillimeters;
                    if (dlina == null)
                    {
                        dlina = elem.Element.LookupParameter("Длина")?.AsDouble() * Consts.FootToMillimeters;
                        if (dlina == null || dlina.Value == 0)
                        {
                            ElementType elementType = elem.Element.Document.GetElement(elem.Element.GetTypeId()) as ElementType;
                            dlina = elementType.LookupParameter("ADSK_Размер_Длина")?.AsDouble() * Consts.FootToMillimeters;
                        }
                    }
                    if (dlina != null)
                        PM += dlina.Value;
                }

            }

            PM = Math.Round(PM / 1000, Rounded);
            return new VolumeInfo { Value = PM, Info = $"{PM} п.м." };
        }

        const double _pgm = 11700.0;

        private double GetAddedPm(double length, double diametr) => (int)(length / _pgm) * 40 * diametr;
        private double GetAddedPmMk2(double check, double length, double diametr) => check > _pgm ? (int)(length / _pgm) * 40 * diametr : 0.0;
        private double GetAddedPmMk2Raw(double check, double length, double diametr) => check > _pgm ? length / _pgm * 40 * diametr : 0.0;
        private double GetKg(double length, double diametr) => length / 1000 * GetMassByDiametr(diametr);
        private VolumeInfo CalcKG(List<AnnexElement> elements)
        {
            double Pgm = 0;
            double? DiametrSterjnya = null;
            List<double> dlini = new List<double>();
            List<double> addedl = new List<double>();
            Dlina = 0.0d;
            double added = 0;
            foreach (var elem in elements)
            {
                DiametrSterjnya = elem.Element.LookupParameter("Диаметр стержня")?.AsDouble() * Consts.FootToMillimeters;
                var ADSK_RazmerDiametr = elem.Element.LookupParameter("ADSK_Размер_Диаметр")?.AsDouble() * Consts.FootToMillimeters;

                if (DiametrSterjnya != null || ADSK_RazmerDiametr != null)
                {
                    var elemTypedid = elem.Element.Document.GetElement(elem.Element.GetTypeId());

                    var needToPm = elemTypedid.get_Parameter(new Guid(ADSK_V_pogonnih_metrax));
                    var counts = elem.Element.LookupParameter("Количество")?.AsInteger();

                    var DlinaSterjnya = elem.Element.LookupParameter("Длина стержня")?.AsDouble() * Consts.FootToMillimeters;
                    var PolnayaDlinaSterjnya = elem.Element.LookupParameter("Полная длина стержня")?.AsDouble() * Consts.FootToMillimeters;
                    var ADSK_RazmerDlina = elem.Element.LookupParameter("ADSK_Размер_Длина")?.AsDouble() * Consts.FootToMillimeters;

                    double dlina = 0;
                    double diametr = 0;


                    if (ADSK_RazmerDlina.GetValueOrDefault() != default)
                        dlina = ADSK_RazmerDlina.Value;

                    if (ADSK_RazmerDiametr.GetValueOrDefault() != default)
                        diametr = ADSK_RazmerDiametr.Value;

                    if (DiametrSterjnya.GetValueOrDefault() != default)
                        diametr = DiametrSterjnya.Value;

                    if (PolnayaDlinaSterjnya.GetValueOrDefault() != default)
                        dlina = PolnayaDlinaSterjnya.Value;

                    double dlinasterjna = 0;
                    if (DlinaSterjnya.GetValueOrDefault() != default)
                        dlinasterjna = DlinaSterjnya.Value;

                    var c = counts != null && counts.Value != 0 ? counts.Value : 1;
                    if (needToPm != null && needToPm.AsInteger() == 1)
                    {
                        added += GetAddedPmMk2(dlinasterjna, dlina, diametr)/* * c*/;
                        dlina += GetAddedPmMk2(dlinasterjna, dlina, diametr);
                    }
                    DiametrSterjnya = diametr;
                    Dlina += dlina/* * c*/;
                }
                else
                {
                    var ADSK_RazmerObshaya = elem.Element.LookupParameter("ADSK_Размер общая")?.AsDouble() * Consts.FootToMillimeters;
                    var ADSK_Massa_Pgm = elem.Element.LookupParameter("ADSK_Масса п.м.")?.AsDouble();

                    Pgm += ADSK_RazmerObshaya.Value * ADSK_Massa_Pgm.Value / 1000;
                }
            }
            Pgm = Math.Round(
                ((Pgm == 0 && DiametrSterjnya != null) ? GetKg(Dlina, DiametrSterjnya.Value): Pgm) / 1000, Rounded);

            return new VolumeInfo
            {
                Value = Pgm,
                Info = $"{Pgm} тн. {{d={DiametrSterjnya}мм l={Math.Round(Dlina, Rounded)}мм add={added}мм}}",
                VolumeType = VolumeInfoTypeEnum.AddedPms,
                Other = added
            };
        }
        private VolumeInfo CalcRiser(List<AnnexElement> elements)
        {
            int? riserCount = 0;
            var sortByType = elements.GroupBy(x =>
                ((((x.Element as FamilyInstance).
                SuperComponent as FamilyInstance).
                SuperComponent as FamilyInstance).Id,
                x.Element.Document)).ToList();
            for (int i = 0; i < sortByType.Count; i++)
            {
                var sorted = sortByType[i].GroupBy(x => x.RPKShipher.F).Count();
                riserCount += sorted;
            }

            return new VolumeInfo
            {
                Value = riserCount.Value,
                Info = $"общее количество стояков {riserCount.Value} | семейств {sortByType.Count}"
            };
        }

        private VolumeInfo GetResources(EnumVolumeType code, List<AnnexElement> elements)
        {
            switch (code)
            {
                case EnumVolumeType.Count:
                    return CalcPieces(elements);

                case EnumVolumeType.M2:
                    return CalcM2(elements);

                case EnumVolumeType.M3:
                    return CalcM3(elements);

                case EnumVolumeType.Pm:
                    return CalcPM(elements);

                case EnumVolumeType.Kg:
                    return CalcKG(elements);

                case EnumVolumeType.Riser:
                    return CalcRiser(elements);

                default:
                    return new VolumeInfo { Value = 0, Info = "ERROR" };
            }
        }

        public VolumeInfo GetVolume(List<AnnexElement> elements)
        {
            Clear();
            Element element = default;
            Parameter workInType = default;
            foreach (var item in elements)
            {
                element = item.Element;
                workInType = element.Document.GetElement(element.GetTypeId())?.LookupParameter("Тип учета работы в типе");
                if (workInType != null)
                    break;
            }
            if (workInType == null)
            {
                _warnings.Append($"{elements[0].WorkType.Name} | {elements[0].GroupOfWork.GroupingRSO} | {elements[0].RPKShipher.Code} - отсутствует тип учета работы в типе\n");
                return new VolumeInfo { Value = 0, Info = NonValid };
            }


            var prevCode = (EnumVolumeType)workInType.AsInteger();
            var code = GetRigthCode(prevCode, elements[0].RPKShipher);
            if (prevCode != code)
                _warnings.Append($"Для элемента {elements[0].RPKShipher.Code} указан {prevCode}({(int)prevCode}), но используется {code}({(int)code})\n");
            if (code == EnumVolumeType.MaterialType)
            {
                var materialIds = element.GetMaterialIds(false);
                foreach (var id in materialIds)
                {
                    var m = element.Document.GetElement(id) as Autodesk.Revit.DB.Material;
                    if (m != null)
                    {
                        var typeMaterial = m.LookupParameter("Тип учета работы")?.AsInteger();
                        if (typeMaterial != null)
                            return GetResources((EnumVolumeType)typeMaterial.Value, elements);
                    }
                }
            }
            else
            {
                return GetResources(code, elements);
            }
            return new VolumeInfo { Value = 0, Info = NonValid };
        }

        private EnumVolumeType GetRigthCode(EnumVolumeType code, RPKShipher shipher)
        {
            if (shipher.M == "СА")
                return EnumVolumeType.M2;
            if (shipher.C == "НСТ")
            {
                if (shipher.M == "КРЧ" && shipher.X_M == "1НФК")
                {
                    if (shipher.P == "250") return EnumVolumeType.M3;
                    if (shipher.P == "65") return EnumVolumeType.M2; // было Count
                }
                return EnumVolumeType.M2;
            }
            if (Regex.IsMatch(shipher.X_M, @"\d{1}С.*"))
                return EnumVolumeType.Count;
            /*if (shipher.M == "ТИЗ" && shipher.X_M == "ЭППС_ECO")
                return EnumVolumeType.M3;*/
            if (shipher.M == "ВБ")
                return EnumVolumeType.Riser;
            return code;
        }

        public void Clear()
        {
            _warnings.Clear();
            Dlina = 0.0;
        }
    }
}
