using MathCalcPrice.Entity;
using MathCalcPrice.RevitsUtils;
using MathCalcPrice.Service.Interfaces;
using MathCalcPrice.StaticResources;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MathCalcPrice.Service
{
    public class CalculatorTemplate : ICalculatorTemplate
    {
        #region Чушня с переменными и одним мини-классом
        internal class GrouppedCalculatedData
        {
            public CalculatorEnitity CalculatorEnitity { get; set; }
            public ClassMaterial ClassMaterial { get; set; }
            public double Volume { get; set; }
        }
        public readonly MaterialsDB _db;
        private readonly List<string> _warnings = new List<string>();
        private readonly List<GrouppedCalculatedData> _calculatedData = new List<GrouppedCalculatedData>();
        private readonly List<CalculatorUpdateEntity> _calculatorUpdateEntities = new List<CalculatorUpdateEntity>();
        private readonly string _pathToTemplate = "";
        #endregion

        public CalculatorTemplate(string pathToTemplate)
        {
            _pathToTemplate = pathToTemplate;
        }

        public string Save(string Path)
        {
            _calculatorUpdateEntities.Clear();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var path = System.IO.Path.ChangeExtension(Path, "xlsx");

            using (var p = new ExcelPackage(new System.IO.FileInfo(_pathToTemplate)))
            {
                int line = 5;
                var worksheet = p.Workbook.Worksheets["Калькулятор затрат"];
                line = WriteMainInformation(worksheet, line);

                Update(_pathToTemplate, "", _calculatorUpdateEntities, Path);
            }
            return path;
        }

        public bool Create(List<AnnexElement> annex, int countOfThreads)
        {
            try
            {
                Groupping(annex.Where(x => x.CalculatorEnitity != default).ToList());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Update(string template, string realObjectName, List<CalculatorUpdateEntity> data, string Path)
        {
            var path = System.IO.Path.ChangeExtension(Path.TrimEnd('.') + "RSO.", "xlsx");
            using (var p = new ExcelPackage(new System.IO.FileInfo(template)))
            {
                Dictionary<int, string> WorkNames = new Dictionary<int, string>();
                var worksheet = p.Workbook.Worksheets["Справочник цен"];

                if (worksheet is null) throw new Exception("Шаблон калькулятора: Лист 'Справочник цен' или 'Калькулятор затрат' не был найден");
                else
                {
                    int y = 3;
                    while (worksheet.Cells[$"C{y}"].Value != null)
                    {
                        WorkNames.Add(y, worksheet.Cells[$"C{y}"].Value.ToString());
                        y++;
                    }

                    for (int i = 0; data.Count > i; i++)
                    {
                        foreach (var item in WorkNames)
                        {
                            if (item.Value == data[i].WorkName)
                            {
                                if (worksheet.Cells[$"{SelectedObjects.SelectedCalcObject.Positions[3]}{item.Key}"].Value.ToString().Trim(' ') == "Проект")
                                {
                                    var Item = data[i];
                                    if (Item is null) continue;

                                    worksheet.Cells[$"{SelectedObjects.SelectedCalcObject.Positions[0]}{item.Key}"].Value = Item.Volume;
                                    worksheet.Cells[$"{SelectedObjects.SelectedCalcObject.Positions[1]}{item.Key}"].Value = Item.Value;
                                    worksheet.Cells[$"{SelectedObjects.SelectedCalcObject.Positions[2]}{item.Key}"].Value = Item.MaterialCost;
                                }
                            }
                        }
                    }
                    p.SaveAs(new System.IO.FileInfo(path));
                }
            }

            return true;
        }
        private void Groupping(List<AnnexElement> annex)
        {
            var periods = annex
                .GroupBy(x => new { x.WorkingPeriod.Period, x.WorkingPeriod.Priority })
                .OrderBy(x => x.Key.Priority).ToArray();

            for (int i = 0; i < periods.Length; i++)
            {
                // выше/ниже нуля
                var phases = periods[i]
                    .GroupBy(x => new { x.Phase.Phase, x.Phase.Priority })
                    .OrderBy(x => x.Key.Priority).ToArray();
                for (int j = 0; j < phases.Length; j++)
                {
                    // фаза строительства
                    var groupsOfWork = phases[j]
                        .OrderBy(x => x.GroupOfWork.Priority)
                        .GroupBy(x => x.GroupOfWork.GroupingRSO)
                        .ToArray();
                    for (int k = 0; k < groupsOfWork.Length; k++)
                    {
                        // группировка работы
                        var workType = groupsOfWork[k]
                            .GroupBy(x => new { x.WorkType.TypeRSOKP, x.WorkType.Priority })
                            .OrderBy(x => x.Key.Priority).ToArray();
                        for (int l = 0; l < workType.Length; l++)
                        {
                            // тип работ
                            var g = workType[l].GroupBy(x => x.RPKShipher.ShortCode);
                            if (workType[l].ElementAt(0)?.RPKShipher.M == "ВБ")
                                g = workType[l].GroupBy(x => x.RPKShipher.M);

                            foreach (var groupped in g)
                            {
                                // материалы
                                var listOfGrouppedElements = groupped.ToList();
                                var leaderElement = listOfGrouppedElements[0];
                                Volume v = new Volume();
                                var volume = v.GetVolume(listOfGrouppedElements);
                                if (v.Warnings.Length != 0)
                                    _warnings.Add(v.Warnings);
                                if (volume.Info == Volume.NonValid)
                                    continue;
                                if (volume.VolumeType == VolumeInfoTypeEnum.Windows && volume.Other != null)
                                {
                                    AddWindows(leaderElement.CalculatorEnitity, leaderElement.RPKShipher, volume);
                                }
                                else
                                {
                                    _calculatedData.Add(new GrouppedCalculatedData
                                    {
                                        CalculatorEnitity = leaderElement.CalculatorEnitity,
                                        Volume = volume.Value,
                                        ClassMaterial = leaderElement.ClassMaterial
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
        private void AddWindows(CalculatorEnitity entity, RPKShipher shipher, VolumeInfo volume)
        {
            WindowsVolumeResult r = volume.Other;
            var xm = shipher.X_M;
            if (r.SquareOpen > 0)
            {
                shipher.X_M = $"{xm}_О";
                var cm = _db.FindAllClassMaterial().FirstOrDefault(x => shipher.CompareTo(x.RPKShipher, nonstrict: true) != RPKShipherCompEnum.NotEqual);
                if (cm != default)
                    _calculatedData.Add(new GrouppedCalculatedData
                    {
                        CalculatorEnitity = entity,
                        ClassMaterial = cm,
                        Volume = r.SquareOpen
                    });
            }
            if (r.SquareClose > 0)
            {
                shipher.X_M = $"{xm}_Г";
                var cm = _db.FindAllClassMaterial().FirstOrDefault(x => shipher.CompareTo(x.RPKShipher, nonstrict: true) != RPKShipherCompEnum.NotEqual);
                if (cm != default)
                {
                    _calculatedData.Add(new GrouppedCalculatedData
                    {
                        CalculatorEnitity = entity,
                        ClassMaterial = cm,
                        Volume = r.SquareClose
                    });
                }
            }
        }
        public MaterialsDB Get_db()
        {
            ExcelDataService dataService = new ExcelDataService();
            return dataService.LoadData();
        }

        #region Заполнение _calculatorUpdateEntities из уже сделанных методов т.к. переделывать два с половиной года 
        private int WriteGroup(ExcelWorksheet worksheet, int line, string workType, string workName, List<GrouppedCalculatedData> data)
        {
            var units = new List<string>();
            double volume = 0.0;
            double priceMaterial = 0.0;
            double priceWork = 0.0;
            double resultPriceMaterial = 0.0;
            double resultPriceWork = 0.0;
            int countofNonZeroWork = 0;
            int countofNonZeroMaterial = 0;
            var innergroup = data.GroupBy(x => new
            {
                x.ClassMaterial.Units,
                x.ClassMaterial.PriceForMaterial,
                x.ClassMaterial.PriceForWork
            });
            foreach (var innerg in innergroup)
            {
                units.Add(innerg.Key.Units);
                var v = innerg.Sum(x => x.Volume);
                volume += v;
                priceMaterial += innerg.Key.PriceForMaterial;
                priceWork += innerg.Key.PriceForWork;
                countofNonZeroMaterial += innerg.Key.PriceForMaterial > 1.0e-12 ? 1 : 0;
                countofNonZeroWork += innerg.Key.PriceForWork > 1.0e-12 ? 1 : 0;
                resultPriceMaterial += v * innerg.Key.PriceForMaterial;
                resultPriceWork += v * innerg.Key.PriceForWork;
            }

            _calculatorUpdateEntities.Add(new CalculatorUpdateEntity
            {
                WorkType = workType,
                WorkName = workName,
                Value = resultPriceWork + resultPriceMaterial,//итоговая цена
                Volume = volume,//итоговый объем 
                MaterialCost = resultPriceMaterial //Цена материала
            });
            line++;
            return line;
        }
        private int WriteMainInformation(ExcelWorksheet worksheet, int line)
        {
            var groupped = _calculatedData.GroupBy(x => new
            {
                x.CalculatorEnitity.WorkName,
                x.CalculatorEnitity.WorkType
            }).ToList();

            foreach (var g in groupped)
            {
                var workType = g.Key.WorkType;
                var workName = g.Key.WorkName;
                line = WriteGroup(worksheet, line, workType, workName, g.ToList());
            }
            return line;
        }
        #endregion
    }
}
