using MathCalcPrice.Service.OneDriveControllers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MathCalcPrice.StaticResources
{
    public static class Paths
    {
        public static string IpAdress { get { return Task.Run(async () => await OneDriveController.GetIpAdress("MathCalcPriceServer")).Result; } }

        public static readonly string MainDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RSO");

        public static readonly string AddinDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Autodesk", "Revit", "Addins", "2019", "RSOaddin");

        public static string CalcDbExcelPath { get; } = Path.Combine(MainDir, "bd_calc.xlsx");

        public static string CalcDbTemplateExcelPath = Path.Combine(MainDir, "calc_template.xlsx");

        public static readonly string ResultPath = Path.Combine(MainDir, "Result");

        static Paths()
        {
            if (!Directory.Exists(MainDir))
                Directory.CreateDirectory(MainDir);
            if (!Directory.Exists(ResultPath))
                Directory.CreateDirectory(ResultPath);
        }
    }
}
