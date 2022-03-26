using MathCalcPrice.StaticResources;
using MathCalcPrice.ViewModels.Entity;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MathCalcPrice.Service.OneDriveControllers
{
	public class OneDriveController : BaseOneDrive
	{
		public static async Task<string> GetIpAdress(string NameProject)
		{
			//01ADEVTEQDRFDBBDE2MFELD2BKOHNBUWL7
			var me = SingAndReturnMe();
			var joinedTeams = await me.Me.JoinedTeams.Request().GetAsync();
			var a2 = await me.Groups[joinedTeams.Where(el => el.DisplayName == "ООО \"Прогресс\"").FirstOrDefault().Id]
				.Drive.Items["01ADEVTEQDRFDBBDE2MFELD2BKOHNBUWL7"].Workbook.Worksheets["Пути"].Range("A1:B10").Request().GetAsync();

			var itemsFromOneDrive = a2.Text.ToObject<string[][]>();

			for (int i = 0; i < itemsFromOneDrive.Length; i++)
			{
				if (itemsFromOneDrive[i][0] == NameProject)
				{
					return itemsFromOneDrive[i][1];
				}
			}

			return null;
		}
		public static async Task<string[]> GetPathToSaveObjectAsync()
		{
			//01ADEVTETHFZD5W3FJKVH2KWKWYSR6F54V
			var me = SingAndReturnMe();
			var joinedTeams = await me.Me.JoinedTeams.Request().GetAsync();
			var a2 = await me.Groups[joinedTeams.Where(el => el.DisplayName == "ООО \"Прогресс\"").FirstOrDefault().Id]
				.Drive.Items["01ADEVTETHFZD5W3FJKVH2KWKWYSR6F54V"].Workbook.Worksheets["Пути"].Range("A1:P4").Request().GetAsync();

			var itemsFromOneDrive = a2.Text.ToObject<string[][]>();

			for (int i = 0; i < itemsFromOneDrive.Length; i++)
			{
                if (itemsFromOneDrive[i][1] == SelectedObjects.SelectedCalcObject.Name.Trim())
                {
					string[] massiveData = new string[5];
					massiveData[0] = itemsFromOneDrive[i][0];
					massiveData[1] = itemsFromOneDrive[i][1];
					massiveData[2] = itemsFromOneDrive[i][2];
					massiveData[3] = itemsFromOneDrive[i][3];
					massiveData[4] = itemsFromOneDrive[i][4];
					return massiveData;
                }
			}
			return null;
		}

		public static async Task<string> DowloandExcelFile(string fileId, string saveName)
		{
            var me = SingAndReturnMe();
			Stream file = null;

			var joinedTeams = await me.Me.JoinedTeams.Request().GetAsync();
            file = await me.Groups[joinedTeams.Where(el => el.DisplayName == "BIM Отдел").FirstOrDefault().Id].Drive.Items[fileId].Content.Request().GetAsync();
            var fullName = Path.Combine(Paths.MainDir, saveName);
            using (var fileStream = new FileStream(fullName, FileMode.Create, FileAccess.ReadWrite))
            {
                file.CopyTo(fileStream);
            }
            file.Dispose();
            return fullName;
        }
		public static async Task DowloandExcelFiles(string saveNameCalcTempalte = "calc_template.xlsx", string bd_calc = "bd_calc.xlsx")
		{
			var me = SingAndReturnMe();
			Stream file = null;

			var joinedTeams = await me.Me.JoinedTeams.Request().GetAsync();
			file = await me.Groups[joinedTeams.Where(el => el.DisplayName == "ООО \"Прогресс\"").FirstOrDefault().Id].Drive.Items["01ADEVTET6J647IDZNJVB2N56SMNAJ7YAT"].Content.Request().GetAsync();
			var fullName = Path.Combine(Paths.MainDir, saveNameCalcTempalte);
			using (var fileStream = new FileStream(fullName, FileMode.Create, FileAccess.ReadWrite))
			{
				file.CopyTo(fileStream);
			}
			file.Dispose();

			file = await me.Groups[joinedTeams.Where(el => el.DisplayName == "BIM Отдел").FirstOrDefault().Id].Drive.Items["01N2KAJ4PKFONUSVN45JAIKMK75GGS4BIG"].Content.Request().GetAsync();
			fullName = Path.Combine(Paths.MainDir, bd_calc);
			using (var fileStream = new FileStream(fullName, FileMode.Create, FileAccess.ReadWrite))
			{
				file.CopyTo(fileStream);
			}
			file.Dispose();
		}
		public static async Task<bool> SaveResultsAsync(string path, string pathInOneDrive)
		{
			string Path = Regex.Replace(DateTime.Now.ToString("dd.MM.yy.HH.mm.ss"), "[^a-zA-Z0-9% ._]", string.Empty);
			var me = SingAndReturnMe();
			byte[] data = System.IO.File.ReadAllBytes(path);
			try
			{
				using (Stream stream = new MemoryStream(data))
				{
					var g = me.Groups["892eb031-5560-4d2c-9142-0030091aabfa"].Drive.Items["01ADEVTERILDNCFS47CFF2CRDOJA7E5XWJ"];
					await me.Groups["892eb031-5560-4d2c-9142-0030091aabfa"]
						.Drive.Items["01ADEVTERILDNCFS47CFF2CRDOJA7E5XWJ"]
						.ItemWithPath($"{Path}CalcPrice.xlsx").Content.Request().PutAsync<DriveItem>(stream);
				};
			}
			catch (Exception ex)
			{
				using (Stream stream = new MemoryStream(data))
				{
					await me.Groups["fa78a005-e9e8-4aa4-b01a-94d0d0c19fc5"]
						.Drive.Items["01N2KAJ4LSMVSN6XNUTJDLX2BM66IZQDGA"]
						.ItemWithPath($"{Path}CalcPrice.xlsx").Content.Request().PutAsync<DriveItem>(stream);
				};

				throw;
			}
			return true;
		}
	}
}

