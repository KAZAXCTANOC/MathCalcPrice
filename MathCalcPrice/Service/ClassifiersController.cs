using Autodesk.Revit.DB;
using MathCalcPrice.Entity;
using MathCalcPrice.StaticResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Service
{
    static class ClassifiersController
    {
        static public List<ParameterClassifiers> GetParameterClassifiers(List<Element> elements)
        {
            IEnumerable<IGrouping<string, Element>> groupByCaterotyName = elements.GroupBy(el => el.Category.Name).Where(el => NeedCategoryList.NeedCategorys.Contains(el.Key));

            List<ParameterClassifiers> parameterClassifiers = new List<ParameterClassifiers>();

            foreach (var group in groupByCaterotyName)
            {
                foreach (var groupElement in group)
                {
                    parameterClassifiers.Add(new ParameterClassifiers
                    {
                        ClassParams = groupElement.LookupParameter("Классификатор_Параметры").AsString(),
                        ClassConstruction = groupElement.LookupParameter("Классификатор_Конструкция").AsString(),
                        ClassMaterial = groupElement.LookupParameter("Классификатор_Материал").AsString(),
                        ClassSection = groupElement.LookupParameter("Классификатор_Секция").AsString(),
                        ClassFloor = groupElement.LookupParameter("Классификатор_Этаж").AsString(),
                        ClassChars = groupElement.LookupParameter("Классификатор_Характеристика_материала").AsString(),
                        GroupName = group.Key,
                        Id = groupElement.Id.ToString()
                    });
                }
            }

            return parameterClassifiers;
        }
        static public List<Groups> GetValidatedSortabledClassifiers(List<ParameterClassifiers> parameterClassifiers)
        {
            List<Groups> groups = new List<Groups>();
            var lists = parameterClassifiers.Where(el => el.IsValid() == false).GroupBy(el => el.GroupName).ToList();
            foreach (var item in lists)
            {
                Groups newGroups = new Groups();
                newGroups.GroupName = item.Key;
                foreach (var newGroupElement in item)
                {
                    newGroups.parameterClassifiers.Add(newGroupElement);
                }
                groups.Add(newGroups);
            }
            return groups;
        }

        static public List<Element> GetFilteredElementCollector(Document doc)
        {
            List<Element> elements = new List<Element>();
            FilteredElementCollector collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();

            foreach (Element e in collector)
            {
                if (null != e.Category)
                {
                    elements.Add(e);
                }
            }

            return elements;
        }
    }
}
