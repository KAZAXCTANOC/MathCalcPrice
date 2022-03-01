using MathCalcPrice.Service.OneDriveControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testes
{
    class Program
    {
        static async Task Main(string[] args)
        {
            _ = await OneDriveController.GetIpAdress("192.168.9.33:8888");
        }
    }
}
