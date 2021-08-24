using System;
using System.Collections.Generic;

namespace MathCalcPrice.RevitsUtils
{
    public enum ClassificatorTypeEnum
    {
        Construction,
        Material,
        CharsMaterial,
        Params,
        Section,
        Floor
    };

    public class ClassificatorsCache
    {
        [LiteDB.BsonId]
        public Guid Id { get; set; }
        public Dictionary<ClassificatorTypeEnum, List<string>> Values { get; set; } = new Dictionary<ClassificatorTypeEnum, List<string>>();

        public ClassificatorsCache()
        {
            //Dictionary<ClassificatorTypeEnum, List<string>> Values = new Dictionary<ClassificatorTypeEnum, List<string>>();
            foreach (ClassificatorTypeEnum item in Enum.GetValues(typeof(ClassificatorTypeEnum)))
            {
                Values.Add(item, new List<string>());
            }
            Console.WriteLine("Done");
        }

        public void Add(ClassificatorTypeEnum cType, string value)
        {
            /*if (!Values.ContainsKey(cType))
                Values.Add(cType, new List<string>());*/
            if (!Values[cType].Contains(value))
                Values[cType].Add(value);
        }

        public List<string> GetByType(ClassificatorTypeEnum cType)
            => Values.ContainsKey(cType) ? Values[cType] : default;
    }
}
