using Google.Apis.Download;
using MathCalcPrice.RevitsUtils;
using MathCalcPrice.Service.Interfaces;
using MathCalcPrice.StaticResources;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MathCalcPrice.Service
{
    class ExcelDataService : IExcelDataService
    {
        private MaterialsDB _db;

        public MaterialsDB LoadData(bool IsGoogle = true)
        {
            try
            {
                if (IsGoogle)
                {
                    LoadDataFromGoogleTable();
                    if (_db == null) throw new NotImplementedException("Выгрузка прошла не слишком удачно, т.к. база пустая");
                    else
                    {
                        MessageBox.Show("fgdfgd");
                        return _db;
                    }

                }
                else
                {
                    LoadDataFromExcel();
                    if (_db == null) throw new NotImplementedException("Выгрузка прошла не слишком удачно, т.к. база пустая");
                    else return _db;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        #region MethodsInterfaces
        public void LoadDataFromExcel()
        {
            throw new NotImplementedException("Эта функиця вам точно нужна ?)");
        }

        public void LoadDataFromGoogleTable()
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
        }

        private bool CheckStatus(IDownloadProgress progress, string path)
        {
            return progress.Status == Google.Apis.Download.DownloadStatus.Completed;
        }
        #endregion
    }
}
