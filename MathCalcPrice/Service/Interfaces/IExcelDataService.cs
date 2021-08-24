using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Service.Interfaces
{
    public interface IExcelDataService
    {
        void LoadDataFromExcel();
        void LoadDataFromGoogleTable();
    }
}
