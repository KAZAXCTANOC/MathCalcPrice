using MathCalcPrice.StaticResources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Service.MathCalcPriceServerController
{
    public class ServerController
    {
        /// <summary>
        /// Сохранения рельтатов в общий зачет расчета РСО
        /// </summary>
        /// <param name="FilePath">Путь до подсчитанного РСО</param>
        /// <param name="NameObject">Наименование объекта под которым они сохранится</param>
        /// <param name="C1">Столбец Объема</param>
        /// <param name="C2">Столбец Стоисости работ</param>
        /// <param name="C3">Столбец Стоимости материала</param>
        /// <param name="Range">Ячейки в которые и сохранится рерультат</param>
        /// <param name="URL">Путь до пазвернутого сервера</param>
        public void SaveToServer(string FilePath, string RevitFileName, string NameObject, string C1, string C2, string C3, string Range = "C3:C102")
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri($"http://{Paths.IpAdress}");
                using (var multipartFormDataContent = new MultipartFormDataContent())
                {
                    var values = new[]
                    {
                        new KeyValuePair<string, string>("RevitFileName", RevitFileName),
                        new KeyValuePair<string, string>("NameObject", $"{NameObject}"),
                    };

                    foreach (var keyValuePair in values)
                    {
                        multipartFormDataContent.Add(new StringContent(keyValuePair.Value), String.Format("\"{0}\"", keyValuePair.Key));
                    }

                    multipartFormDataContent.Add(new ByteArrayContent(File.ReadAllBytes(FilePath)), '"' + "File" + '"', Guid.NewGuid().ToString());

                    var otvet = client.PostAsync("DataBase/SendFile", multipartFormDataContent).Result;
                    //                                   $"DataBase/UpdateTable?NameObject=Текущий объект (Атмосфера 3)&Range=C3:C102&Col1=E&Col2=F&Col3=G"
                    var saveUpdateOtvet = client.GetAsync($"DataBase/UpdateTable?NameObject={NameObject}&Range={Range}&Col1={C1}&Col2={C2}&Col3={C3}").Result;
                }
            }
        }

        public Task<string[]> GetObjectData(string nameObject)
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri($"http://192.168.69.69:8888");
                using (var multipartFormDataContent = new MultipartFormDataContent())
                {
                    return client.GetAsync($"DataBase/GetObjectData?NameObject={nameObject}").Result.Content.ReadAsAsync<string[]>();
                }
            }
        }
    }
}
