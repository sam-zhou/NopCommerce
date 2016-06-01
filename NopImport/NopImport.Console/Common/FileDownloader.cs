using System;
using System.IO;
using System.Net;

namespace NopImport.Console.Common
{
    public class FileDownloader
    {
        private readonly string _localFolder;

        public FileDownloader(string localFolder)
        {
            _localFolder = localFolder;
        }

        public bool DownloadFile(string url, string fileName)
        {
            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile(url, Path.Combine(_localFolder, fileName));
                }
                catch (Exception ex)
                {
                    return false;
                }
                
            }
            return true;
        }
    }
}
