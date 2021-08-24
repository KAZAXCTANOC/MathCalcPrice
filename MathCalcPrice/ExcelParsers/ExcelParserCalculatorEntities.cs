using ExcelDataReader;
using MathCalcPrice.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace MathCalcPrice.ExcelParsers
{
    internal class ExcelParserCalculatorEntities
    {
        public string FileName { get; private set; } = default;

        private DataSet GetDataSet()
        {
            if (!File.Exists(FileName)) throw new Exception($"Отсутствует файл {FileName}");
            using (var stream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    return reader.AsDataSet();
                }
            }
            throw new Exception($"Ошибка во время чтения файла {FileName}");
        }
        private T ConvertCell<T>(object element) where T : IConvertible
        {
            if (element is DBNull) return default;
            if (typeof(T) == typeof(double))
            {
                double.TryParse(element.ToString().Replace('.', ','), out double tmp);
                element = tmp;
            }

            return (T)element;
        }

        private RPKShipher CreateShipher(DataRow row) =>
            new RPKShipher()
            {
                S = $"{row[0]}",
                F = $"{row[1]}",
                C = $"{row[2]}",
                M = $"{row[3]}",
                X_M = $"{row[4]}",
                P = $"{row[5]}"
            };

        private CalculatorEnitity GetCalculatorEnitity(DataRow row)
        {
            var t = ConvertCell<string>(row[6]);
            var n = ConvertCell<string>(row[7]);
            var l = ConvertCell<string>(row[8]);
            return new CalculatorEnitity
            {
                RPKShipher = CreateShipher(row),
                WorkType = t is null ? "" : t.Trim(),
                WorkName = n is null ? "" : n.Trim(),
                LegacyParam = l is null ? "" : l.Trim(),
                Priority = (int)(ConvertCell<double>(row[9]))
            };
        }
        public List<CalculatorEnitity> Read(string fileName)
        {
            FileName = fileName;
            List<CalculatorEnitity> calculatorEntities = new List<CalculatorEnitity>();
            var dataset = GetDataSet();
            foreach (DataTable table in dataset.Tables)
            {
                if (table.TableName.Trim() == "DB")
                {
                    for (int i = 2; i < table.Rows.Count; i++)
                    {
                        calculatorEntities.Add(GetCalculatorEnitity(table.Rows[i]));
                    }
                }
            }

            return calculatorEntities;
        }
    }
}
