﻿using Google.Apis.Download;
using MathCalcPrice.Entity;
using MathCalcPrice.RevitsUtils;
using MathCalcPrice.Service;
using MathCalcPrice.Service.OneDriveControllers;
using MathCalcPrice.StaticResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Path = System.IO.Path;

namespace MathCalcPrice
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Settings _settings = new Entity.Settings();
        private LinkFile _mainFile;
        public CalculatorTemplate wr = new CalculatorTemplate(Paths.CalcDbTemplateExcelPath);
        private MaterialsDB _db;

        public MainWindow(LinkFile linkFile)
        {
            InitializeComponent();

            _mainFile = linkFile;
            _settings.LinkedFiles = _mainFile.GetDocuments(true).Select(x => new LinkFile(x)).OrderBy(x => x.Name).ToList();
        }

        private async Task CreateExcelRSOAsync()
        {
            await LoadDataFromOneDrive();

            CalculatorTemplate wr = new CalculatorTemplate(Paths.CalcDbTemplateExcelPath);

            var readyForRecording = GetElemsForRSO();

            wr.Create(readyForRecording.AnnexResult, Environment.ProcessorCount);

            var s = wr.Save(Path.Combine(Paths.ResultPath, $"RSO.{_mainFile.Name}{DateTime.Now}.xls".Replace(".rvt", "_").Replace(' ', '.').Replace(':', '.')));

            if (OnCloud.IsChecked == true)
            {
                await OneDriveController.SaveResultsAsync(s, $"RSO.{_mainFile.Name}{DateTime.Now}.xls");
            }

            MessageBox.Show($"Файл сохранен по пути {Path.Combine(Paths.ResultPath, $"RSO.{_mainFile.Name}{DateTime.Now}.xls".Replace(".rvt", "_").Replace(' ', '.').Replace(':', '.'))}");

            _db?.Dispose(); // освобождение хендлов
        }

        public (List<AnnexElement> AnnexResult, List<(ElementTemp e, string docName)> NonValid) GetElemsForRSO()
        {
            //SpinLock sl = new SpinLock();
            List<List<AnnexElement>> rawAnnex = new List<List<AnnexElement>>();
            List<List<(ElementTemp element, string docName)>> rawNonValid = new List<List<(ElementTemp element, string docName)>>();
            Annex annex = new Annex(_db);
            var ext = new ElementGetter(_settings.ClassificatorsCache);
            int count = 0;
            int countOfDocs = _settings.LinkedFiles.Where(x => x.IsChecked).Count();
            Parallel.ForEach(_settings.LinkedFiles, new ParallelOptions { MaxDegreeOfParallelism = countOfDocs > Environment.ProcessorCount ? Environment.ProcessorCount : countOfDocs },
                (doc) => {
                    if (!doc.IsChecked) return;

                    var raw = ext.Work(doc.Doc, _db);
                    var result = annex.ElementsHandling(raw.elems.Where(x => x.Valid).ToList(), _settings);
                    //sl.Lock(); 
                    count++;
                    rawAnnex.Add(result.result);
                    rawNonValid.Add(raw.elems.Where(x => !x.Valid).Select(x => (x, doc.Name)).ToList());
                    rawNonValid.Add(result.nonValid.Select(x => (x, doc.Name)).ToList());
                    //sl.Unlock();
                }
            );
            return (rawAnnex.SelectMany(x => x).ToList(), rawNonValid.SelectMany(x => x).ToList());
        }

        private async Task LoadDataFromOneDrive()
        {
            string pathPrices, pathGroups;

            OneDriveController oneDriveController = new OneDriveController();
            pathPrices = await OneDriveController.DowloandExcelFile(Cache.SelectedCostViewElement, "database_prices_progress.xlsx");
            pathGroups = await OneDriveController.DowloandExcelFile(Cache.SelectedJobViewElement, "spisok-grupp.xlsx");

            var pathCalcDb = Paths.CalcDbExcelPath;
            _db = Cache.Ensure(pathPrices, pathGroups, pathCalcDb, Paths.MainDir);
            //isdone = true;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _db?.Dispose(); // освобождение хендлов
            
            await CreateExcelRSOAsync();
        }

        private void IsOneDriveCheked_Checked(object sender, RoutedEventArgs e)
        {
            if (IsOneDriveCheked.IsChecked == true)
            {
                SelectFileButton.IsEnabled = false;
                TreeViewControl tree = new TreeViewControl();
                tree.ShowDialog();
                if (Cache.SelectedTreeViewElement != null)
                {
                    FilePath.Text = $"Файл с MicrosoftOneDrive: {Cache.SelectedTreeViewElement.Title}";
                    Paths.CalcDbTemplateExcelPath = Task.Run(() => OneDriveController.DowloandExcelFile(Cache.SelectedTreeViewElement, "calc_template.xlsx")).Result;
                }
            }
            else
            {
                FilePath.Text = null;
                SelectFileButton.IsEnabled = true;
            }
        }

        private void IsOneDriveCheked_Checked_1(object sender, RoutedEventArgs e)
        {
            FilePath.Text = null;
            SelectFileButton.IsEnabled = true;
        }
    }
}
