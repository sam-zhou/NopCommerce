using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Plugin.ExternalAuth.QQ
{
    public class QQExternalAuthMethod : BasePlugin, IExternalAuthenticationMethod
    {
        private readonly ISettingService _settingService;

        public QQExternalAuthMethod(ISettingService settingService)
        {
            this._settingService = settingService;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ExternalAuthQQ";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.ExternalAuth.QQ.Controllers" }, { "area", null } };
        }

        public void GetPublicInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "ExternalAuthQQ";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.ExternalAuth.QQ.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// 安装插件
        /// </summary>
        public override void Install()
        {
            // settings
            var settings = new QQExternalAuthSettings()
            {
                AppId = "",
                AppSecret = ""
            };
            _settingService.SaveSetting(settings);

            // locales
            this.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.QQ.Login", "使用QQ微信登录");
            this.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.QQ.AppId", "唯一凭证");
            this.AddOrUpdatePluginLocaleResource("Plugins.ExternalAuth.QQ.AppSecret", "唯一凭证密钥");

            base.Install();
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        public override void Uninstall()
        {
            // settings
            _settingService.DeleteSetting<QQExternalAuthSettings>();

            this.DeletePluginLocaleResource("Plugins.ExternalAuth.QQ.Login");
            this.DeletePluginLocaleResource("Plugins.ExternalAuth.QQ.AppId");
            this.DeletePluginLocaleResource("Plugins.ExternalAuth.QQ.AppSecret");
            this.DeletePluginLocaleResource("Plugins.FriendlyName.ExternalAuth.QQ");

            base.Uninstall();
        }
    }
}
