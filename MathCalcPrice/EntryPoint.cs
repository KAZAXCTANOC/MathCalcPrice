using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MathCalcPrice.Entity;
using MathCalcPrice.RevitsUtils;
using MathCalcPrice.Service;
using MathCalcPrice.StaticResources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Google.Apis.Download;
using Microsoft.Win32;
using System.Threading;

namespace MathCalcPrice
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class EntryPoint : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                LinkFile linkFile = new LinkFile(uiApp.ActiveUIDocument.Document);

                StaticLinkedFile.linkFile = linkFile;

                Thread thread = new Thread(() => {
                    Window window = new MainWindow(linkFile);
                    window.ShowDialog();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                if(thread.IsAlive)
                {
                    thread.Abort();
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
                return Result.Succeeded;
            }
            return Result.Succeeded;
        }
    }
}
