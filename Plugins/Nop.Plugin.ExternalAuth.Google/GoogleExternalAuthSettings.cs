using Nop.Core.Configuration;

namespace Nop.Plugin.ExternalAuth.Google
{
    public class GoogleExternalAuthSettings : ISettings
    {
        public string ClientKeyIdentifier { get; set; }
        public string ClientSecret { get; set; }
    }
}
