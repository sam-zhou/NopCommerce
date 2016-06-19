using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace Nop.Services.Infrastructure
{
    public static class UrlHelper
    {
        public static string GetBaseUrl(this IWebHelper helper)
        {
            var result = "http";

            if (helper.IsCurrentConnectionSecured())
            {
                result += "s";
            }

            result += "://";

            result += helper.GetStoreHost(false).Replace("http://","");

            return result;
        }
    }
}
