using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Helpers
{
    public class HttpUtil
    {

        private const string SContentType = "application/x-www-form-urlencoded";
        private const string SUserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        public static string Send(string data, string url)
        {
            return Send(Encoding.GetEncoding("UTF-8").GetBytes(data), url);
        }

        public static string Send(byte[] data, string url)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null)
            {
                throw new ApplicationException(string.Format("Invalid url string: {0}", url));
            }
            // request.UserAgent = sUserAgent;  
            request.ContentType = SContentType;
            request.Method = "POST";
            request.ContentLength = data.Length;
            var requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            try
            {
                var responseStream = request.GetResponse().GetResponseStream();
                var str = string.Empty;
                if (responseStream != null)
                {
                    using (var reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8")))
                    {
                        str = reader.ReadToEnd();
                    }
                    responseStream.Close();
                }
                
                
                return str;

            }
            catch (Exception exception)
            {
                throw exception;
            }
            
            
        }
    }
}
