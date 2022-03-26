using MathCalcPrice.Entity;
using MathCalcPrice.RevitsUtils;
using MathCalcPrice.Service;
using MathCalcPrice.Service.MathCalcPriceServerController;
using MathCalcPrice.Service.OneDriveControllers;
using MathCalcPrice.StaticResources;
using MathCalcPrice.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        private string RevitFileSaveName = "";
        private ServerController serverController = new ServerController();

        public MainWindow(LinkFile linkFile, List<Groups> parameterClassifiers = null)
        {
            InitializeComponent();

            var mwvm = new MainWindowViewModel();
            mwvm.ParameterClassifiers = parameterClassifiers;
            this.DataContext = mwvm;

            var revitFileNameMass = linkFile.Name.Split('_');
            for (int i = 0; i < revitFileNameMass.Length - 1; i++) { RevitFileSaveName += revitFileNameMass[i]; }
            _mainFile = linkFile;
            _settings.LinkedFiles = _mainFile.GetDocuments(false).Select(x => new LinkFile(x)).OrderBy(x => x.Name).ToList();

            Task.Run(async () => await LoadDataFromOneDrive()).Wait();

            Task.Run(async () => await OneDriveController.DowloandExcelFiles()).Wait();

            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _db?.Dispose(); // освобождение хендлов
        }

        private async Task CreateExcelRSOAsync()
        {
            CalculatorTemplate wr = new CalculatorTemplate(Paths.CalcDbTemplateExcelPath);
            var readyForRecording = GetElemsForRSO();

            wr.Create(readyForRecording.AnnexResult, Environment.ProcessorCount);

            var saveString = wr.Save(Path.Combine(Paths.ResultPath, $"RSO.{_mainFile.Name}{DateTime.Now}.xls".Replace(".rvt", "_").Replace(' ', '.').Replace(':', '.')));

            if (OnCloud.IsChecked == true)
            {
                var objectPoperty = await serverController.GetObjectDataAsync(SelectedObjects.SelectedCalcObject.Name);
                serverController.SaveToServer(
                    Path.Combine(saveString),
                        RevitFileSaveName, objectPoperty[0], objectPoperty[1], objectPoperty[2], objectPoperty[3], objectPoperty[4]);
            }

            _db?.Dispose(); // освобождение хендлов
            MessageBox.Show($"Расчеты закончены и сохранены по пути {saveString}", "Завершение", MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }
        public (List<AnnexElement> AnnexResult, List<(ElementTemp e, string docName)> NonValid) GetElemsForRSO()
        {
            List<List<AnnexElement>> rawAnnex = new List<List<AnnexElement>>();
            List<List<(ElementTemp element, string docName)>> rawNonValid = new List<List<(ElementTemp element, string docName)>>();
            Annex annex = new Annex(_db);
            var ext = new ElementGetter(_settings.ClassificatorsCache);
            int count = 0;
            int countOfDocs = _settings.LinkedFiles.Where(x => x.IsChecked).Count();
            Parallel.ForEach(_settings.LinkedFiles, new ParallelOptions { MaxDegreeOfParallelism = countOfDocs > Environment.ProcessorCount ? Environment.ProcessorCount : countOfDocs },
                (doc) =>
                {
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
            foreach (var item in rawAnnex)
            {
                foreach (var item2 in item)
                {
                    item2.ClassMaterial.ToString();
                }
            }
            return (rawAnnex.SelectMany(x => x).ToList(), rawNonValid.SelectMany(x => x).ToList());
        }
        private async Task LoadDataFromOneDrive()
        {
            string pathPrices, pathGroups;

            pathPrices = await OneDriveController.DowloandExcelFile("01N2KAJ4PBJXRHT5QQ6ZCYPTTKRYQJ4BRY", "database_prices_progress.xlsx");
            pathGroups = await OneDriveController.DowloandExcelFile("01N2KAJ4KMDIY67A4TFNC3MT4WNQIQMQ3Q", "spisok-grupp.xlsx");

            var pathCalcDb = Paths.CalcDbExcelPath;
            _db = Cache.Ensure(pathPrices, pathGroups, pathCalcDb, Paths.MainDir);

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _db?.Dispose(); // освобождение хендлов
            Task.Run(async () => await LoadDataFromOneDrive()).Wait();
            await CreateExcelRSOAsync();
        }
    }
}
