using MathCalcPrice.Entity;
using MathCalcPrice.RevitsUtils;
using System.Collections.Generic;

namespace MathCalcPrice.Service.Interfaces
{
    interface ICalculatorTemplate
    {
        bool Create(List<AnnexElement> annex, int countOfThreads = 3);
        string Save(string path);
        bool Update(string path, string objectNameToUpdate, List<CalculatorUpdateEntity> calculatorUpdateEntities);
        MaterialsDB Get_db();
    }
}
