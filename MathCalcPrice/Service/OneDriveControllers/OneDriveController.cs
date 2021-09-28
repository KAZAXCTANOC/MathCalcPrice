using MathCalcPrice.StaticResources;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Service.OneDriveControllers
{
    public class OneDriveController : BaseOneDrive
    {
        public static async Task<string> DowloandExcelFile(string fileName, string saveName)
        {
            try
            {
                GraphServiceClient graphClient = OneDriveController.SingAndReturnMe();
                
                var driveItem = await graphClient.Me.Drive.SharedWithMe().Request().GetAsync();

                var aw2 = await graphClient
                            .Drives[driveItem.Where(E => E.Name == fileName).FirstOrDefault().RemoteItem.ParentReference.DriveId]
                            .Items[driveItem.Where(E => E.Name == fileName).FirstOrDefault().RemoteItem.Id]
                            .Content.Request().GetAsync();

                var fullName = Path.Combine(Paths.MainDir, saveName);

                using (var fileStream = new FileStream(fullName, FileMode.Create, FileAccess.ReadWrite))
                {
                    aw2.CopyTo(fileStream);
                }

                aw2.Dispose();

                return fullName;
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
