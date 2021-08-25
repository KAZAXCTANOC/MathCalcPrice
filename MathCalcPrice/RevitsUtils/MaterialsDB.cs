using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MathCalcPrice.Entity;

namespace MathCalcPrice.RevitsUtils
{
    public class MaterialsDB : IDisposable
    {
        public string DbName = @"excel.docs.cache.db";
        private bool _exist = false;

        public LiteDatabase Db;

        public MaterialsDB(string path = default)
        {
            if (path != default) 
                DbName = Path.Combine(path, DbName);
            Db = new LiteDatabase(DbName);
            _exist = true;
        }


        public void DropCollections()
        {
            Db.DropCollection("ElementTemp");
            Db.DropCollection("ExcelDocument");
            DropExcelDbPrices();
            DropExcelDbGroupOfWork();
            DropExcelCalcDb();
        }

        public void DropExcelDbPrices()
        {
            Db.DropCollection("ClassMaterial");
        }

        public void DropExcelDbGroupOfWork()
        {
            Db.DropCollection("ConstructionPhase");
            Db.DropCollection("WorkingPeriod");
            Db.DropCollection("WorkType");
            Db.DropCollection("GroupingOfWork");
        }

        public void DropExcelCalcDb()
        {
            Db.DropCollection("CalculatorEnitity");
        }

        public void InsertMany(List<ClassMaterial> classMaterials)
        {
            var classMaterialcollection = Db.GetCollection<ClassMaterial>("ClassMaterial");
            foreach (var cm in classMaterials)
            {
                classMaterialcollection.Insert(cm);
            }
            classMaterialcollection.EnsureIndex(x => x.RPKShipher.Code);
        }

        public ILiteCollection<ExcelDocument> GetExcelCollection() => Db.GetCollection<ExcelDocument>("ExcelDocument");

        public void InsertOne(ExcelDocument excelDoc)
        {
            var excelDocuments = Db.GetCollection<ExcelDocument>("ExcelDocument");
            excelDocuments.Insert(excelDoc);
            excelDocuments.EnsureIndex(x => x.Name);
        }

        public void InsertOne(ClassificatorsCache cache)
        {
            var excelDocuments = Db.GetCollection<ClassificatorsCache>("ClassificatorsCache");
            excelDocuments.Insert(cache);
            excelDocuments.EnsureIndex(x => x.Values);
        }

        public bool UpdateClassificatorsCache(ClassificatorsCache cache)
        {
            var excelDocuments = Db.GetCollection<ClassificatorsCache>("ClassificatorsCache");
            return excelDocuments.Update(cache);
        }

        public bool EnsureExcelDoc(string hash)
        {
            var findOne = Db.GetCollection<ExcelDocument>("ExcelDocument").FindOne(x => hash.Equals(x.Hash));
            return findOne != null /*&& findOne.LastChange == excelDoc.LastChange*/;
        }
        public void InsertMany(List<AnnexElement> elems)
        {
            var annexElements = Db.GetCollection<AnnexElement>("AnnexElement");
            foreach (var el in elems)
            {
                annexElements.Insert(el);
            }
            annexElements.EnsureIndex(x => x.RPKShipher);
        }

        public void InsertMany(List<ElementTemp> elems)
        {
            var elemsdb = Db.GetCollection<ElementTemp>("ElementTemp");
            foreach (var el in elems)
            {
                elemsdb.Insert(el);
            }
            elemsdb.EnsureIndex(x => x.Code);
        }

        public void InsertMany(List<ConstructionPhase> elems)
        {
            var elemsdb = Db.GetCollection<ConstructionPhase>("ConstructionPhase");
            foreach (var e in elems)
            {
                elemsdb.Insert(e);
            }
            elemsdb.EnsureIndex(x => x.RPKShipher);
        }

        public void InsertMany(List<WorkingPeriod> elems)
        {
            var elemsdb = Db.GetCollection<WorkingPeriod>("WorkingPeriod");
            foreach (var e in elems)
            {
                elemsdb.Insert(e);
            }
            elemsdb.EnsureIndex(x => x.RPKShipher);
        }
        public void InsertMany(List<WorkType> elems)
        {
            var elemsdb = Db.GetCollection<WorkType>("WorkType");
            foreach (var e in elems)
            {
                elemsdb.Insert(e);
            }
            elemsdb.EnsureIndex(x => x.RPKShipher);
        }
        public void InsertMany(List<GroupingOfWork> elems)
        {
            var elemsdb = Db.GetCollection<GroupingOfWork>("GroupingOfWork");
            foreach (var e in elems)
            {
                elemsdb.Insert(e);
            }
            elemsdb.EnsureIndex(x => x.RPKShipher);
        }

        public void InsertMany(List<CalculatorEnitity> elems)
        {
            var elemsdb = Db.GetCollection<CalculatorEnitity>("CalculatorEnitity");
            foreach (var e in elems)
            {
                elemsdb.Insert(e);
            }
            elemsdb.EnsureIndex(x => x.RPKShipher);
        }
        public List<CalculatorEnitity> FindAllCalculatorEnitity()
        {
            var res = Db.GetCollection<CalculatorEnitity>("CalculatorEnitity");
            return res.FindAll().ToList();
        }

        public List<ElementTemp> FindAllElementTemp()
        {
            var res = Db.GetCollection<ElementTemp>("ElementTemp");
            return res.FindAll().ToList();
        }

        public List<ClassMaterial> FindAllClassMaterial()
        {
            var collection = Db.GetCollection<ClassMaterial>("ClassMaterial");
            return collection.FindAll().ToList();
        }
        public async Task<List<ClassMaterial>> FindAllClassMaterialAsync()
        {
            List<ClassMaterial> result = new List<ClassMaterial>();
            await Task.Run(() => {
                var collection = Db.GetCollection<ClassMaterial>("ClassMaterial");
                result = collection.FindAll().ToList();
            });
            return result;
        }
        public List<GroupingOfWork> FindAllGroupingOfWork()
        {
            var res = Db.GetCollection<GroupingOfWork>("GroupingOfWork");
            return res.FindAll().ToList();
        }
        public List<WorkType> FindAllWorkType()
        {
            var res = Db.GetCollection<WorkType>("WorkType");
            return res.FindAll().ToList();
        }
        public List<WorkingPeriod> FindAllWorkingPeriod()
        {
            var res = Db.GetCollection<WorkingPeriod>("WorkingPeriod");
            return res.FindAll().ToList();
        }
        public List<ConstructionPhase> FindAllConstructionPhase()
        {
            var res = Db.GetCollection<ConstructionPhase>("ConstructionPhase");
            return res.FindAll().ToList();
        }
        public ClassificatorsCache FindClassificatorsCache()
        {
            var res = Db.GetCollection<ClassificatorsCache>("ClassificatorsCache");
            var ls = res.FindAll().ToList();
            if (ls.Count > 0) return ls[0];

            var cache = new ClassificatorsCache();
            InsertOne(cache);
            return cache;
        }

        public List<AnnexElement> FindAllAnnexElement()
        {
            var res = Db.GetCollection<AnnexElement>("AnnexElement");
            return res.FindAll().ToList();
        }

        public void Commit()
        {
            if (_exist)
                Db.Commit();
        }

        public void Dispose()
        {
            if (_exist)
            {
                Db.Commit();
                Db.Dispose();
            }
            _exist = false;
        }
    }
}
