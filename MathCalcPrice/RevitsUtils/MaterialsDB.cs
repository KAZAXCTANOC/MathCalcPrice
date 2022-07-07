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

        public bool EnsureExcelDoc(string hash)
        {
            var findOne = Db.GetCollection<ExcelDocument>("ExcelDocument").FindOne(x => hash.Equals(x.Hash));
            return findOne != null /*&& findOne.LastChange == excelDoc.LastChange*/;
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

        public List<ClassMaterial> FindAllClassMaterial()
        {
            var collection = Db.GetCollection<ClassMaterial>("ClassMaterial");
            return collection.FindAll().ToList();
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
