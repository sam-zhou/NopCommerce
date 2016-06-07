using Nop.Core.Plugins;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using System.Web.Routing;

namespace Nop.Plugin.ExternalAuth.Google
{
    public class GoogleExternalAuthMethod : BasePlugin, IExternalAuthenticationMethod
    {
        #region Fields

        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public GoogleExternalAuthMethod(ISettingService settingService)
        {
            this._settingService = settingService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ExternalAuthGoogle";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.ExternalAuth.Google.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for displaying plugin in public store
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPublicInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "ExternalAuthGoogle";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.ExternalAuth.Google.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new GoogleExternalAuthSettings()
            {
                ClientKeyIdentifier = "",
                ClientSecret = "",
            };
            _settingService.SaveSetting(settings);

            //locales

            this.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.Google.Description", "<h3>Configuring Google OAuth2</h3><br/><h4>Google Developers Console</h4><ul><li>Go to the <a href=\"https://console.developers.google.com/\" target=\"_blank\">Google Developers Console</a> and log in with your Google Developer Account</li><li>Select \"Create Project\"</li><li>Go to APIs & Auth -> Credentials in the left-hand navigation panel</li><li>Select \"Create new Client ID\" in the OAuth Panel</li><li>In the creation panel:<ul><li>Select \"Web application\" as Application Type</li><li>Set \"Authorized JavaScript origins\" to the URL of your nopCommerce site (http://www.yourStore.com)</li><li>Set \"Authorized redirect URI\" to URL of login callback (http://www.yourStore.com/plugins/externalauthGoogle/logincallback/)</li></ul></li><li>Then go to APIs & Auth -> Consent Screen and fill out</li><li>Now get your API key (Client ID and Client Secret) and configure your nopCommerce</li></ul><p>For more details, read the Google docs: <a href=\"https://developers.google.com/accounts/docs/OAuth2\">Using OAuth 2.0 to Access Google APIs</a>.</p>");
            this.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.Google.Login", "Login using Google account");
            this.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.Google.ClientKeyIdentifier", "Client ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.Google.ClientKeyIdentifier.Hint", "Enter your Client ID key here. You can find it on Google Developers console page.");
            this.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.Google.ClientSecret", "Client Secret");
            this.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.Google.ClientSecret.Hint", "Enter your client secret here. You can find it on your Google Developers console page.");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<GoogleExternalAuthSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.ExternalAuth.Google.Description");
            this.DeletePluginLocaleResource("Plugins.ExternalAuth.Google.Login");
            this.DeletePluginLocaleResource("Plugins.ExternalAuth.Google.ClientKeyIdentifier");
            this.DeletePluginLocaleResource("Plugins.ExternalAuth.Google.ClientKeyIdentifier.Hint");
            this.DeletePluginLocaleResource("Plugins.ExternalAuth.Google.ClientSecret");
            this.DeletePluginLocaleResource("Plugins.ExternalAuth.Google.ClientSecret.Hint");

            base.Uninstall();
        }
        
        #endregion
    }
}
