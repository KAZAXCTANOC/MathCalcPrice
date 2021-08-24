using Autodesk.Revit.DB;
using MathCalcPrice.RevitsUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    public class Annex
    {
        private readonly List<WorkingPeriod> _wp;
        private readonly List<ConstructionPhase> _cp;
        private readonly List<GroupingOfWork> _gop;
        private readonly List<WorkType> _wt;
        private readonly List<ClassMaterial> _cm;
        private readonly List<CalculatorEnitity> _ce;

        public Annex(MaterialsDB db)
        {
            _wp = db.FindAllWorkingPeriod();
            _cp = db.FindAllConstructionPhase();
            _gop = db.FindAllGroupingOfWork();
            _wt = db.FindAllWorkType();
            _cm = db.FindAllClassMaterial();
            _ce = db.FindAllCalculatorEnitity();
        }

        private bool IsMonitoring(ElementTemp item)
        {
            bool? tmp = item.Element.LookupParameter("Рабочий набор")?.AsString() == "Мониторинг";
            return item.Element.IsMonitoringLinkElement() || (tmp.HasValue && tmp.Value);
        }

        private bool ADSK_vozvodimost(ElementTemp item)
        {
            if (item.Code.C == "ВДВ" || item.Code.C == "НДВ")
                return item.Element.LookupParameter("ADSK_Возводимость")?.AsInteger() == 0;
            return false;
        }

        public bool Filtering(ElementTemp elem, Settings settings, bool allFiltersIsOff)
        {
            bool applyOnlyOneFilter = settings.FiltrationType == FiltrationType.UseOnlyOne;
            foreach (var filter in settings.Filters)
            {
                // удовлетворяет хотя бы одному фильтру
                if (applyOnlyOneFilter && filter.IsEnabled && elem.Code.CompareTo(filter.RPKShipher) == RPKShipherCompEnum.Equal)
                    return true;
                // удовлетворяет всем фильтрам
                if (!applyOnlyOneFilter && filter.IsEnabled && elem.Code.CompareTo(filter.RPKShipher) != RPKShipherCompEnum.Equal)
                    return false;
            }
            return (applyOnlyOneFilter && allFiltersIsOff) || !applyOnlyOneFilter;
        }

        public (List<AnnexElement> result, List<ElementTemp> nonValid) ElementsHandling(List<ElementTemp> elems, Settings settings)
        {
            bool allFiltersIsOff = settings?.Filters.Where(x => x.IsEnabled).Count() == 0;
            var result = new List<AnnexElement>();
            var nonValid = new List<ElementTemp>();
            foreach (var item in elems)
            {
                if (!(settings is null) && !Filtering(item, settings, allFiltersIsOff))
                {
                    item.NonValidInfo = "Отброшен при фильтрации";
                    nonValid.Add(item);
                    continue;
                }

                var workingPeriod = _wp.FirstOrDefault(x => item.Code.CompareTo(x.RPKShipher) == RPKShipherCompEnum.Equal);//1
                var phase = _cp.FirstOrDefault(x => item.Code.CompareTo(x.RPKShipher) == RPKShipherCompEnum.Equal); // 1
                var group = _gop.FirstOrDefault(x => item.Code.CompareTo(x.RPKShipher) == RPKShipherCompEnum.Equal); // 1
                var calculatorEnitity = _ce.FirstOrDefault(x => item.Code.CompareTo(x.RPKShipher, true) == RPKShipherCompEnum.Equal); // 1
                var workType = _wt.FirstOrDefault(x => item.Code.CompareTo(x.RPKShipher, true) == RPKShipherCompEnum.Equal); // 1
                var classmaterial = _cm.FirstOrDefault(x => item.Code.CompareTo(x.RPKShipher) == RPKShipherCompEnum.SemiEqual); // 0
                bool isMonitoring = IsMonitoring(item);
                bool vozvodymost = ADSK_vozvodimost(item);

                if (workingPeriod != default && phase != default && group != default && workType != default && classmaterial != default && !isMonitoring && !vozvodymost)
                {
                    result.Add(new AnnexElement
                    {
                        WorkingPeriod = workingPeriod,
                        Phase = phase,
                        GroupOfWork = group,
                        WorkType = workType,
                        RPKShipher = new RPKShipher
                        {
                            S = item.Code.S,
                            F = item.Code.F,
                            M = classmaterial.RPKShipher.M,
                            X_M = classmaterial.RPKShipher.X_M,
                            C = classmaterial.RPKShipher.C,
                            P = classmaterial.RPKShipher.P
                        },///*item.Code,*/classmaterial.RPKShipher,
                        ClassMaterial = classmaterial,
                        Element = item.Element,
                        CalculatorEnitity = calculatorEnitity
                    });
                }
                else
                {
                    item.NonValidInfo = $"Период: {workingPeriod != default} |" +
                    $" Этап: {phase != default} |" +
                    $" Группировка: {group != default} |" +
                    $" Тип: {workType != default} |" +
                    $" Материалы: {classmaterial != default}" +
                    (isMonitoring ? $" [Мониторинг]" : "") +
                    (vozvodymost ? $" [ADSK_Возводимость]" : "");
                    nonValid.Add(item);
                }
            }
            return (result, nonValid);
        }
    }
}
