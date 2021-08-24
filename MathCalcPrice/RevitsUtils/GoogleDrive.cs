using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using MathCalcPrice.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MathCalcPrice.RevitsUtils
{
    public class GoogleDrive
    {

        /// <summary>
        ///   Поиск локализованного ресурса типа System.Byte[].
        /// </summary>

        private readonly string[] _scopes = { DriveService.Scope.Drive };
        public string ApplicationName { get; } = "Google Drive API .NET Downloader";

        private readonly DriveService _service = default;
        private string _docId = default;
        public readonly string _secret = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Autodesk", "Revit", "Addins", "2019", "RSOaddin", "service-secret.json");

        public GoogleDrive()
        {

            GoogleCredential credential;
            using(var stream = new MemoryStream(Resources.service_secret))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(_scopes);
            }
            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public GoogleDrive Set(string docId)
        {
            _docId = docId;
            return this;
        }

        public async Task<Google.Apis.Drive.v3.Data.File> GetMetaInfoAsync()
        {
            if (_docId != default && _service != default)
            {
                var req = _service.Files.Get(_docId);
                req.Fields = "*";
                return await req.ExecuteAsync();
            }
            return default;
        }

        public async Task<IDownloadProgress> DownloadExcelAsync(string outFileName)
        {
            if (_docId == default || _service == default)
                return default;

            IDownloadProgress result = null;
            using (var output = new FileStream(outFileName, FileMode.OpenOrCreate))
            {
                result = await _service.Files
                    .Export(_docId, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    .DownloadAsync(output);
            }
            return result;
        }
        public IDownloadProgress DownloadExcel(string outFileName)
        {
            if (_docId == default || _service == default)
                return default;

            IDownloadProgress result = null;
            using (var output = new FileStream(outFileName, FileMode.OpenOrCreate))
            {
                result = _service.Files.Export(_docId, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet").DownloadWithStatus(output);
            }
            return result;
        }

        public IDownloadProgress DownloadRaw(string outFileName)
        {
            if (_docId == default || _service == default)
                return default;

            IDownloadProgress result = null;
            using (var output = new FileStream(outFileName, FileMode.OpenOrCreate))
            {
                result = _service.Files.Get(_docId)
                    .DownloadWithStatus(output);
            }
            return result;
        }

        private string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
            {
                mimeType = regKey.GetValue("Content Type").ToString();
            }
            else if (ext == ".png")
            {
                mimeType = "image/png";
            }
            return mimeType;
        }


        public IUploadProgress UploadFile(string fileName, string description, string parent)
        {
            var body = new Google.Apis.Drive.v3.Data.File
            {
                Description = description,
                MimeType = GetMimeType(fileName),
                Name = Path.GetFileName(fileName),
                Parents = new List<string> { parent }
            };
            var req = _service.Files.Create(body, new MemoryStream(File.ReadAllBytes(fileName)), GetMimeType(fileName));
            return req.Upload();
        }
    }
}
