using System.Web;
using System.Web.Routing;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.WaterMark.Core;
using Nop.Plugin.Misc.WaterMark.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.WaterMark
{
	public class WaterMarkPlugin : BasePlugin, IMiscPlugin
	{
		private readonly ISettingService _settingService;
		private readonly ILogger _logger;
		private readonly HttpContextBase _httpContext;

		public WaterMarkPlugin(ISettingService settingService, ILogger logger, HttpContextBase httpContext)
		{
			_settingService = settingService;
			_logger = logger;
			_httpContext = httpContext;
		}

		public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "Configure";
			controllerName = "NopWaterMark";
			routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Misc.WaterMark.Controllers" }, { "area", null } };
		}

		void IPlugin.Install()
		{
			//settings
			var settings = new WaterMarkSettings()
			{
				Positions = (int)WaterMarkPositions.Center,
				Enable = false
			};

			_settingService.SaveSetting(settings);

			//locales
			this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.WaterMark.PictureId", "Image for watermark");
			this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.WaterMark.PictureId.Hint", "Upload watermark image for place on images");

			this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.WaterMark.Positions", "Positions of watermark image");
			this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.WaterMark.Positions.Hint", "Select positions where watermark will be placed on image");

			this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.WaterMark.Enable", "Enable watermark");
			this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.WaterMark.Scale", "Image scaling (percents)");
			this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.WaterMark.Transparency", "Transparency of watermark image");
			this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.WaterMark.OnlyLargerThen", "Use only for photos larger then Xpx in one dimension");
			this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.WaterMark.ApplyOnProductPictures", "Should apply watermark on products pictures");
			this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.WaterMark.ApplyOnCategoryPictures", "Should apply watermark on category pictures");
			this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.WaterMark.ApplyOnProductVariantAttributeValuePictures", "Should apply watermark on productvariant pictures");

			base.Install();
		}

		void IPlugin.Uninstall()
		{
			//settings
			_settingService.DeleteSetting<WaterMarkSettings>();

			//locales
			this.DeletePluginLocaleResource("Nop.Plugin.Misc.WaterMark.PictureId");
			this.DeletePluginLocaleResource("Nop.Plugin.Misc.WaterMark.PictureId.Hint");
			this.DeletePluginLocaleResource("Nop.Plugin.Misc.WaterMark.Positions");
			this.DeletePluginLocaleResource("Nop.Plugin.Misc.WaterMark.Positions.Hint");
			this.DeletePluginLocaleResource("Nop.Plugin.Misc.WaterMark.Enable");
			this.DeletePluginLocaleResource("Nop.Plugin.Misc.WaterMark.Scale");
			this.DeletePluginLocaleResource("Nop.Plugin.Misc.WaterMark.Transparency");
			this.DeletePluginLocaleResource("Nop.Plugin.Misc.WaterMark.OnlyLargerThen");
			this.DeletePluginLocaleResource("Nop.Plugin.Misc.WaterMark.ApplyOnProductPictures");
			this.DeletePluginLocaleResource("Nop.Plugin.Misc.WaterMark.ApplyOnCategoryPictures");
			this.DeletePluginLocaleResource("Nop.Plugin.Misc.WaterMark.ApplyOnProductVariantAttributeValuePictures");
			((NopPictureService)EngineContext.Current.Resolve<IPictureService>()).ClearThumbs();
			base.Uninstall();
		}
	}
}
