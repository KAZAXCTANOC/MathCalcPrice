using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Download;
using MathCalcPrice.Entity;
using MathCalcPrice.RevitsUtils;
using MathCalcPrice.Service;
using MathCalcPrice.StaticResources;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using Microsoft.Win32;
using OfficeOpenXml;
using MathCalcPrice.ViewModels.Entity;

namespace MathCalcPrice.ViewModels
{
    class MainWindowViewModel : BaseViewModel
    {
        #region Enum
        char[] Collumns = {
            'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };
        #endregion
        private CalcObject selectedCalcObject { get; set; } = null;
        public CalcObject SelectedCalcObject 
        {
            get
            {
                return selectedCalcObject;
            }
            set
            {
                selectedCalcObject = value;
                SelectedObjects.SelectedCalcObject = value;
            }
        }

        public List<CalcObject> CalcObjects { get; set; } = new List<CalcObject>();
        public string Path 
        { 
            get 
            {
                return Paths.CalcDbTemplateExcelPath;
            } 
        }

        public ICommand SetNewPathCommand { get; }
        private void SetNewPathCommandExecute(object p)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel|*.xlsx|Excel 2010|*.xlsx";
            if (openFileDialog.ShowDialog() == true)
            {
                Paths.CalcDbTemplateExcelPath = openFileDialog.FileName;
                
                UbdateCalcObject();
            }
        }

        private void UbdateCalcObject()
        {
            try
            {
                CalcObjects.Clear();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var p = new ExcelPackage(new FileInfo(Path)))
                {
                    var worksheet = p.Workbook.Worksheets["Справочник цен"];

                    for (int i = 0; i < Collumns.Length; i += 4)
                    {
                        if (worksheet.Cells[$"{Collumns[i]}{1}"].Value != null)
                        {
                            List<string> list = new List<string>();

                            list.Add(Collumns[i].ToString());
                            list.Add(Collumns[i+1].ToString());
                            list.Add(Collumns[i+2].ToString());
                            list.Add(Collumns[i+3].ToString());

                            CalcObject calcObject = new CalcObject()
                            {
                                Name = worksheet.Cells[$"{Collumns[i]}{1}"].Value.ToString(),
                                Positions = list
                            };

                            CalcObjects.Add(calcObject);
                        }
                        else
                        {
                            OnPropertyChanged(nameof(CalcObjects));
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public MainWindowViewModel()
        {
            SetNewPathCommand = new LambdaCommand(SetNewPathCommandExecute);
        }
    }
}
