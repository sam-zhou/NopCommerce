using System;
using Nop.Services.Authentication.External;

namespace Nop.Plugin.ExternalAuth.QQ.Core
{
    public interface IQQExternalProviderAuthorizer : IExternalProviderAuthorizer
    {
        Uri GenerateServiceLoginUrl();
    }
}
