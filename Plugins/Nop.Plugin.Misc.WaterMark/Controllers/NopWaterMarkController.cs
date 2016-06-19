using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.WaterMark.Core;
using Nop.Plugin.Misc.WaterMark.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.WaterMark.Controllers
{
	public partial class NopWaterMarkController : BaseController
	{

		private readonly ISettingService _settingService;
		private readonly IPluginFinder _pluginFinder;
		private readonly IPermissionService _permissionService;
		private readonly IStoreService _storeService;
		private readonly IWorkContext _workContext;
		private readonly ICustomerActivityService _customerActivityService;
		private readonly ILocalizationService _localizationService;

		public NopWaterMarkController(
			ISettingService settingService,
			IPluginFinder pluginFinder,
			IPermissionService permissionService,
			IStoreService storeService,
			IWorkContext workContext,
			ICustomerActivityService customerActivityService,
			ILocalizationService localizationService)
			: base()
		{
			_settingService = settingService;
			_pluginFinder = pluginFinder;
			_permissionService = permissionService;
			_storeService = storeService;
			_workContext = workContext;
			_customerActivityService = customerActivityService;
			_localizationService = localizationService;
		}

		[ChildActionOnly]
		[AdminAuthorize]
		public ActionResult Configure()
		{
			int scopeConfiguration = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

			var settings = (WaterMarkSettings)this._settingService.LoadSetting<WaterMarkSettings>(scopeConfiguration);
			var settingsModel = new WaterMarkSettingsModel
				{
					ActiveStoreScopeConfiguration = scopeConfiguration,
					OnlyLargerThen = settings.OnlyLargerThen,
					PictureId = settings.PictureId,
					Scale = settings.Scale,
					Enable = settings.Enable
				};

			foreach (WaterMarkPositions position in Enum.GetValues(typeof(WaterMarkPositions)))
			{
				settingsModel.PositionsValues.Add(new SelectListItem()
				{
					Text = CommonHelper.ConvertEnum(((object)position).ToString()),
					Value = ((object)position).ToString(),
					Selected = ((int)position & settings.Positions) == (int)position
				});
			}

			if (scopeConfiguration > 0)
			{
				settingsModel.Enable_Override = this._settingService.SettingExists(settings, x => x.Enable, scopeConfiguration);
				settingsModel.OnlyLargerThen_Override = this._settingService.SettingExists(settings, x => x.OnlyLargerThen,
																						   scopeConfiguration);
				settingsModel.PictureId_Override = this._settingService.SettingExists(settings, x => x.PictureId,
																						   scopeConfiguration);
				settingsModel.Positions_Override = this._settingService.SettingExists(settings, x => x.Positions,
																						   scopeConfiguration);
				settingsModel.Scale_Override = this._settingService.SettingExists(settings, x => x.Scale,
																						   scopeConfiguration);
				settingsModel.Transparency_Override = this._settingService.SettingExists(settings, x => x.Transparency,
																						   scopeConfiguration);
			}

			return View("~/Plugins/Nop.Plugin.Misc.WaterMark/Views/Settings/Configure.cshtml", settingsModel);
		}

		[HttpPost]
		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult Configure(WaterMarkSettingsModel model)
		{
			if (!((Controller)this).ModelState.IsValid)
				return this.Configure();

			int scopeConfiguration = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

			WaterMarkSettings settings = (WaterMarkSettings)this._settingService.LoadSetting<WaterMarkSettings>(scopeConfiguration);



			var values = ((WaterMarkPositions[])Enum.GetValues(typeof(WaterMarkPositions))).Select(x => x.ToString()).ToArray();

			int removedCount = 0;


			var selectedPositions = this.HttpContext.Request.Form.AllKeys.Where(x => x.StartsWith("PositionsSelectedValues_")).Select(x => x.Replace("PositionsSelectedValues_", string.Empty)).ToArray();

			var positions = string.Join(", ", selectedPositions.ToArray());

			settings.Positions = (int)Enum.Parse(typeof(WaterMarkPositions), positions);
			settings.Enable = model.Enable;
			settings.OnlyLargerThen = model.OnlyLargerThen;
			settings.Scale = model.Scale;
			settings.Transparency = model.Transparency;
			settings.PictureId = model.PictureId;
			settings.ApplyOnCategoryPictures = model.ApplyOnCategoryPictures;
			settings.ApplyOnProductPictures = model.ApplyOnProductPictures;
			settings.ApplyOnProductVariantAttributeValuePictures = model.ApplyOnProductVariantAttributeValuePictures;

			foreach (WaterMarkPositions position in Enum.GetValues(typeof(WaterMarkPositions)))
			{
				model.PositionsValues.Add(new SelectListItem()
				{
					Text = CommonHelper.ConvertEnum(((object)position).ToString()),
					Value = ((object)position).ToString(),
					Selected = ((int)position & settings.Positions) == (int)position
				});
			}

			//if (!string.IsNullOrEmpty(model.Positions))
			//{
			//	settings.Positions = (WaterMarkPositions)Enum.Parse(typeof(WaterMarkPositions), model.Positions);
			//}

			if (model.Enable_Override || scopeConfiguration == 0)
				_settingService.SaveSetting(settings, x => x.Enable, scopeConfiguration, false);
			else if (scopeConfiguration > 0)
				_settingService.DeleteSetting(settings, x => x.Enable, scopeConfiguration);

			if (model.OnlyLargerThen_Override || scopeConfiguration == 0)
				_settingService.SaveSetting(settings, x => x.OnlyLargerThen, scopeConfiguration, false);
			else if (scopeConfiguration > 0)
				_settingService.DeleteSetting(settings, x => x.OnlyLargerThen, scopeConfiguration);

			if (model.Scale_Override || scopeConfiguration == 0)
				_settingService.SaveSetting(settings, x => x.Scale, scopeConfiguration, false);
			else if (scopeConfiguration > 0)
				_settingService.DeleteSetting(settings, x => x.Scale, scopeConfiguration);

			if (model.Transparency_Override || scopeConfiguration == 0)
				_settingService.SaveSetting(settings, x => x.Transparency, scopeConfiguration, false);
			else if (scopeConfiguration > 0)
				_settingService.DeleteSetting(settings, x => x.Transparency, scopeConfiguration);

			if (model.PictureId_Override || scopeConfiguration == 0)
				_settingService.SaveSetting(settings, x => x.PictureId, scopeConfiguration, false);
			else if (scopeConfiguration > 0)
				_settingService.DeleteSetting(settings, x => x.PictureId, scopeConfiguration);

			if (model.Positions_Override || scopeConfiguration == 0)
				_settingService.SaveSetting(settings, x => x.Positions, scopeConfiguration, false);
			else if (scopeConfiguration > 0)
				_settingService.DeleteSetting(settings, x => x.Positions, scopeConfiguration);

			if (model.ApplyOnCategoryPictures_Override || scopeConfiguration == 0)
				_settingService.SaveSetting(settings, x => x.ApplyOnCategoryPictures, scopeConfiguration, false);
			else if (scopeConfiguration > 0)
				_settingService.DeleteSetting(settings, x => x.ApplyOnCategoryPictures, scopeConfiguration);

			if (model.ApplyOnProductPictures_Override || scopeConfiguration == 0)
				_settingService.SaveSetting(settings, x => x.ApplyOnProductPictures, scopeConfiguration, false);
			else if (scopeConfiguration > 0)
				_settingService.DeleteSetting(settings, x => x.ApplyOnProductPictures, scopeConfiguration);

			if (model.ApplyOnProductVariantPictures_Override || scopeConfiguration == 0)
				_settingService.SaveSetting(settings, x => x.ApplyOnProductVariantAttributeValuePictures, scopeConfiguration, false);
			else if (scopeConfiguration > 0)
				_settingService.DeleteSetting(settings, x => x.ApplyOnProductVariantAttributeValuePictures, scopeConfiguration);

			this._settingService.ClearCache();
			((NopPictureService)EngineContext.Current.Resolve<IPictureService>()).ClearThumbs();
			return Configure();
		}
	}
}
