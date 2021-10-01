using MathCalcPrice.StaticResources;
using MathCalcPrice.ViewModels.Entity;
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
        public static async Task<string> DowloandExcelFile(MenuItem file, string saveName)
        {
            try
            {
                var me = SingAndReturnMe();

                var aw2 = await me.Groups["fa78a005-e9e8-4aa4-b01a-94d0d0c19fc5"].Drive.Items[file.Id].Content.Request().GetAsync();

                var fullName = Path.Combine(Paths.MainDir, saveName);

                using (var fileStream = new FileStream(fullName, FileMode.Create, FileAccess.ReadWrite))
                {
                    aw2.CopyTo(fileStream);
                }

                aw2.Dispose();

                return fullName;
            }
            catch (Exception e) { throw e; }
        }

        public async Task<MenuItem> GetDataFromGroupAsync()
        {
            GraphServiceClient Iam = SingAndReturnMe();

            var groups = await Iam.Groups.Request().GetAsync();

            MenuItem root = new MenuItem() { Title = "Меню" };

            foreach (var group in groups.CurrentPage)
            {
                Console.WriteLine($"Группа: {group.DisplayName} Id: {group.Id}");

                if (group.DisplayName == "BIM Отдел")
                {
                    MenuItem childItem1 = new MenuItem() { Title = group.DisplayName, Id = group.Id };

                    var drive1 = await Iam.Groups[group.Id].Drives.Request().GetAsync();

                    foreach (var driveitem1 in drive1)
                    {
                        if (drive1.CurrentPage.Count == 0) continue;
                        var drive2 = await Iam.Groups[group.Id].Drives[driveitem1.Id].Root.Children.Request().GetAsync();

                        foreach (var driveitem2 in drive2)
                        {
                            Console.WriteLine($"\t\t → {driveitem2.ODataType}: {driveitem2.Name} Id: {driveitem2.Id}");

                            //
                            MenuItem childItem2 = new MenuItem() { Title = driveitem2.Name, Id = driveitem2.Id };
                            //
                            var drive3 = await Iam.Groups[group.Id].Drives[driveitem1.Id].Items[driveitem2.Id].Children.Request().GetAsync();

                            foreach (var driveitem3 in drive3)
                            {
                                Console.WriteLine($"\t\t\t → {driveitem3.ODataType}: {driveitem3.Name} Id: {driveitem3.Id}");
                                //
                                MenuItem childItem3 = new MenuItem() { Title = driveitem3.Name, Id = driveitem3.Id };
                                //

                                var drive4 = await Iam.Groups[group.Id].Drives[driveitem1.Id].Items[driveitem3.Id].Children.Request().GetAsync();
                                foreach (var driveitem4 in drive4)
                                {
                                    Console.WriteLine($"\t\t\t\t → {driveitem4.ODataType}: {driveitem4.Name} Id: {driveitem4.Id}");
                                    //
                                    MenuItem childItem4 = new MenuItem() { Title = driveitem4.Name, Id = driveitem4.Id };
                                    //

                                    var drive5 = await Iam.Groups[group.Id].Drives[driveitem1.Id].Items[driveitem4.Id].Children.Request().GetAsync();
                                    foreach (var driveitem5 in drive5)
                                    {
                                        Console.WriteLine($"\t\t\t\t → {driveitem5.ODataType}: {driveitem5.Name} Id: {driveitem5.Id}");
                                        //
                                        MenuItem childItem5 = new MenuItem() { Title = driveitem5.Name, Id = driveitem5.Id };
                                        //
                                        childItem4.Items.Add(childItem5);
                                    }
                                    childItem3.Items.Add(childItem4);
                                }
                                childItem2.Items.Add(childItem3);
                            }
                            childItem1.Items.Add(childItem2);
                        }
                        root.Items.Add(childItem1);
                    }
                }
            }

            return root;
        }
        public static async Task<bool> SaveResultsAsync(string path, string pathInOneDrive)
        {
            var me = SingAndReturnMe();
            byte[] data = System.IO.File.ReadAllBytes(path);

            using (Stream stream = new MemoryStream(data))
            {
                try
                {
                    await me.Groups["fa78a005-e9e8-4aa4-b01a-94d0d0c19fc5"]
                        .Drive.Items["01N2KAJ4LSMVSN6XNUTJDLX2BM66IZQDGA"]
                        .ItemWithPath(path.Remove(1, 29)).Content.Request().PutAsync<DriveItem>(stream);
                }
                catch (Exception)
                {

                    throw;
                }
            }

            return true;
        }

    }
}
