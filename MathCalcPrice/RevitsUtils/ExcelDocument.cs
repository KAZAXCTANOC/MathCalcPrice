using LiteDB;
using System;

namespace MathCalcPrice.RevitsUtils
{
    public class ExcelDocument
    {
        [BsonId]
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Hash { get; set; }
    }
}
