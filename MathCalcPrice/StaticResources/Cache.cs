using System;
using System.IO;
using System.Security.Cryptography;
using Google.Apis.Download;
using Google.Apis.Upload;
using MathCalcPrice.RevitsUtils;
using MathCalcPrice.ExcelParsers;

namespace MathCalcPrice.StaticResources
{
    public static class Cache
    {
        public static string GetHash(byte[] bytes)
        {
            var hash = MD5.Create().ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static string GetHash(string filePath) => GetHash(File.ReadAllBytes(filePath));

        private static void CreateExcelDocs(MaterialsDB db)
        {
            var excelCollections = db.GetExcelCollection();
            if (excelCollections.Count() == 0)
            {
                ExcelDocument prices = new ExcelDocument { Date = DateTime.Now, Hash = "", Name = "Prices" };
                ExcelDocument groups = new ExcelDocument { Date = DateTime.Now, Hash = "", Name = "Groups" };
                ExcelDocument calcDb = new ExcelDocument { Date = DateTime.Now, Hash = "", Name = "CalcDb" };
                excelCollections.Insert(prices);
                excelCollections.Insert(groups);
                excelCollections.Insert(calcDb);
                excelCollections.EnsureIndex(x => x.Name);
                db.Commit();
            }
        }

        private const string _docIdPrices = "1PO-D_5DW9pmE1LXU_rSnQ2EYMUch3UngfzIwx9NyPNw";
        private const string _namePrices = "database_prices_progress.xlsx";

        private const string _docIdGroups = "1aifmNyTTrHoWrSSLsn5pqOj1MXNpOlDHnm0kd9RbUmE";
        private const string _nameGroups = "spisok-grupp.xlsx";

        private const string _uploadToDir = "1tvOQiX0DSC-mch86HIdxSwJLP6ER3DJz";

        private static IDownloadProgress DownloadDocument(string id, string fullname)
        {
            var gd = new GoogleDrive().Set(id);
            return gd.DownloadExcel(fullname);
        }

        public static IUploadProgress Upload(string fileName)
        {
            var gd = new GoogleDrive();
            return gd.UploadFile(fileName, $"РСО калькулятор {Path.GetFileNameWithoutExtension(fileName)}", _uploadToDir);
        }

        public static (string fullname, IDownloadProgress info) DownloadPrices(string localdir)
        {
            var fullname = Path.Combine(localdir, _namePrices);
            File.Delete(fullname);
            return (fullname, DownloadDocument(_docIdPrices, fullname));
        }

        public static (string fullname, IDownloadProgress info) DownloadGroups(string localdir)
        {
            var fullname = Path.Combine(localdir, _nameGroups);
            File.Delete(fullname);
            return (fullname, DownloadDocument(_docIdGroups, fullname));
        }

        public static MaterialsDB Ensure(string pathPrices, string pathListOfGroup, string pathCalcDb, string dbPath)
        {
            MaterialsDB db = new MaterialsDB(dbPath);

            CreateExcelDocs(db);
            try
            {
                var excelCollections = db.GetExcelCollection();
                var hashprices = GetHash(pathPrices);
                if (!db.EnsureExcelDoc(hashprices))
                {
                    var prices = excelCollections.FindOne(x => x.Name == "Prices");
                    prices.Hash = hashprices;
                    prices.Date = DateTime.Now;
                    excelCollections.Update(prices);
                    db.DropExcelDbPrices();
                    var ep = new ExcelParser(pathPrices);
                    var res = ep.Read();
                    db.InsertMany(res);
                }

                var hashGroups = GetHash(pathListOfGroup);
                if (!db.EnsureExcelDoc(hashGroups))
                {
                    var groups = excelCollections.FindOne(x => x.Name == "Groups");
                    groups.Hash = hashGroups;
                    groups.Date = DateTime.Now;
                    excelCollections.Update(groups);
                    db.DropExcelDbGroupOfWork();
                    var e = new ExcelParserListWorkingGroup(pathListOfGroup);
                    e.Read();
                    db.InsertMany(e.WorkTypes);
                    db.InsertMany(e.WorkingPeriods);
                    db.InsertMany(e.GroupingOfWorks);
                    db.InsertMany(e.ConstructionPhases);
                }

                var hashCalcDb = GetHash(pathCalcDb);
                if (!db.EnsureExcelDoc(hashCalcDb))
                {
                    var calcDb = excelCollections.FindOne(x => x.Name == "CalcDb");
                    calcDb.Hash = hashCalcDb;
                    calcDb.Date = DateTime.Now;
                    excelCollections.Update(calcDb);
                    db.DropExcelCalcDb();
                    var e = new ExcelParserCalculatorEntities();
                    var calcDbEntitites = e.Read(pathCalcDb);
                    db.InsertMany(calcDbEntitites);
                }
            }
            catch (Exception ex)
            {
                db.Dispose();
                throw ex;
            }
            return db;
        }
    }
}
