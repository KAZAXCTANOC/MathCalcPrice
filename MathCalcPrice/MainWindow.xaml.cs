using Google.Apis.Download;
using MathCalcPrice.Entity;
using MathCalcPrice.RevitsUtils;
using MathCalcPrice.Service;
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

        private void CreateExcelRSOAsync()
        {
            LoadDataFromExcels();

            CalculatorTemplate wr = new CalculatorTemplate(Paths.CalcDbTemplateExcelPath);

            var readyForRecording = GetElemsForRSO();

            wr.Create(readyForRecording.AnnexResult, Environment.ProcessorCount);

            wr.Save(Path.Combine(Paths.ResultPath, $"RSO.{_mainFile.Name}{DateTime.Now}.xls".Replace(".rvt", "_").Replace(' ', '.').Replace(':', '.')));

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

        private void LoadDataFromExcels()
        {
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    string pathPrices = default, pathGroups = default;
                    var prices = Cache.DownloadPrices(Paths.MainDir);
                    if (CheckStatus(prices.info, prices.fullname))
                        pathPrices = prices.fullname;

                    var groups = Cache.DownloadGroups(Paths.MainDir);
                    if (CheckStatus(groups.info, groups.fullname))
                        pathGroups = groups.fullname;

                    var pathCalcDb = Paths.CalcDbExcelPath;
                    _db = Cache.Ensure(pathPrices, pathGroups, pathCalcDb, Paths.MainDir);
                });
            }).Wait();
        }

        private bool CheckStatus(IDownloadProgress progress, string path)
        {
            return progress.Status == DownloadStatus.Completed;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _db?.Dispose(); // освобождение хендлов

            if(SelectedObjects.SelectedCalcObject != null)
            {
                try
                {
                    await Task.Run(CreateExcelRSOAsync);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(ex.Message);
                    });
                }
            }
            else
            {
                MessageBox.Show("Выберите объект !!");
            }
        }

        private void AnimStart()
        {
            var da = new DoubleAnimation(360, 0, new Duration(TimeSpan.FromSeconds(5)));
            var rt = new RotateTransform(360,75,75);
            MyEllipse.RenderTransform = rt;
            da.RepeatBehavior = RepeatBehavior.Forever;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
        }

        private void AnimStop()
        {
            var da = new DoubleAnimation(360, 0, new Duration(TimeSpan.FromSeconds(5)));
            da.BeginTime = null;
            var rt = new RotateTransform(360, 75, 75);
            MyEllipse.RenderTransform = rt;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
        }
    }
}
