using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Plugin.ExternalAuth.WeiXin.Models;
using Nop.Services.Authentication.External;

namespace Nop.Plugin.ExternalAuth.WeiXin.Core
{
    public interface IWeiXinExternalProviderAuthorizer : IExternalProviderAuthorizer
    {
        AuthorizeState RegisterEmail(string returnUrl, RegisterModel model);
    }
}
