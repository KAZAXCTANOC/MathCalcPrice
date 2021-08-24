using Autodesk.Revit.DB;
using MathCalcPrice.RevitsUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Entity
{
    public class ElementGetter
    {
        private ClassificatorsCache _classificatorsCache;
        public ElementGetter(ClassificatorsCache classificatorsCache = default)
        {
            _classificatorsCache = classificatorsCache;
        }
        private IList<Element> GetAllModelElements(Document doc)
        {
            List<Element> elements = new List<Element>();

            FilteredElementCollector collector
                = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType();

            foreach (Element e in collector)
            {
                if (null != e.Category)
                {
                    elements.Add(e);
                }
            }

            return elements;
        }

        private string CheckEmptyString(string s)
            => s == null || s.Trim().Length == 0 || s.Contains("null") ? "*" : s;
        private (RPKShipher shipher, bool valid) CreateId(Parameter s, Parameter f, Parameter c, Parameter m, Parameter x_m, Parameter p)
        {
            var S = CheckEmptyString(s.AsString());
            var F = CheckEmptyString(f.AsString());
            var C = CheckEmptyString(c.AsString());
            var M = CheckEmptyString(m.AsString());
            var X_M = CheckEmptyString(x_m.AsString());
            var P = CheckEmptyString(p.AsString());
            AddClassificators(S, F, C, M, X_M, P);
            return (
                new RPKShipher
                {
                    S = S,
                    F = F,
                    C = C,
                    M = M,
                    X_M = X_M,
                    P = P
                },
                !(C == "*" && M == "*" && X_M == "*" && P == "*")
                );
        }

        private void AddClassificators(string s, string f, string c, string m, string x_m, string p)
        {
            if (_classificatorsCache == default) return;
            _classificatorsCache.Add(ClassificatorTypeEnum.Section, s);
            _classificatorsCache.Add(ClassificatorTypeEnum.Floor, f);
            _classificatorsCache.Add(ClassificatorTypeEnum.Construction, c);
            _classificatorsCache.Add(ClassificatorTypeEnum.Material, m);
            _classificatorsCache.Add(ClassificatorTypeEnum.CharsMaterial, x_m);
            _classificatorsCache.Add(ClassificatorTypeEnum.Params, p);
        }

        public (RPKShipher shipher, bool valid) GetMaterialShipher(Element e)
        {
            var m = e.LookupParameter(Consts.ClassMaterial);
            if (m == null) return (null, false);
            var c = e.LookupParameter(Consts.ClassConstruction);
            var s = e.LookupParameter(Consts.ClassSection);
            var f = e.LookupParameter(Consts.ClassFloor);
            var x_m = e.LookupParameter(Consts.ClassChars);
            var p = e.LookupParameter(Consts.ClassParams);
            if (s != null && f != null && c != null &&
                x_m != null && p != null)
            {
                return CreateId(s, f, c, m, x_m, p);
            }
            return (null, false);
        }

        private List<ElementTemp> GetNeedToDump(Document doc)
        {
            var needToDump = new List<ElementTemp>();
            var elems = GetAllModelElements(doc);
            foreach (var elem in elems)
            {
                var id = GetMaterialShipher(elem);
                if (id.shipher != null)
                    needToDump.Add(new ElementTemp()
                    {
                        Valid = id.valid,
                        Code = id.shipher,
                        ElementId = elem.Id.IntegerValue,
                        Name = elem.Name,
                        Element = elem
                    });
            }
            return needToDump;
        }

        public (string info, List<ElementTemp> elems) Work(Document doc, MaterialsDB db)
        {
            if (doc == null || db == null) return (null, null);
            var result = GetNeedToDump(doc);
            //db.InsertMany(result);
            return ($"{Path.GetFileName(doc.PathName)} Обработано элементов: {result.Count}", result);
        }

    }
}
