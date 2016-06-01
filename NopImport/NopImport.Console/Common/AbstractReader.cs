using System;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace NopImport.Console.Common
{
    public abstract class AbstractReader
    {
        private FileDownloader _fileDownloader;

        protected virtual FileDownloader FileDownloader
        {
            get
            {
                if (_fileDownloader == null)
                {
                    var path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\Presentation\\Nop.Web\\Content\\Images\\Thumbs"));
                    _fileDownloader = new FileDownloader(path);
                }
                return _fileDownloader;
            }
        }

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;


        protected void ChangeProgress(int percentage)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, new ProgressChangedEventArgs(percentage, null));
            }
        }

        public abstract void Process();

        protected string ReadHtml(string url)
        {

            var request = WebRequest.Create(url);
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var sr = new StreamReader(responseStream))
                        {
                            var content = sr.ReadToEnd();

                            return content;
                        }
                    }
                }
            }
            return null;
        }
    }
}
