using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Plugins;
using Nop.Plugin.ExternalAuth.WeiXin.Core;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.ExternalAuth.WeiXin.Models;

namespace Nop.Plugin.ExternalAuth.WeiXin.Controllers
{
    public class ExternalAuthWeiXinController : BasePluginController
    {
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IWeiXinExternalProviderAuthorizer _weiXinExternalProviderAuthorizer;

        public ExternalAuthWeiXinController(ISettingService settingService,
            IPermissionService permissionService, IStoreContext storeContext,
            IStoreService storeServie, IWorkContext workContext,
            ILocalizationService localizationService, IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings, IPluginFinder pluginFinder,IWeiXinExternalProviderAuthorizer weiXinExternalProviderAuthorizer)
        {
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._storeContext = storeContext;
            this._storeService = storeServie;
            this._workContext = workContext;
            this._localizationService = localizationService;
            _openAuthenticationService = openAuthenticationService;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _pluginFinder = pluginFinder;
            _weiXinExternalProviderAuthorizer = weiXinExternalProviderAuthorizer;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return Content("Access denied");

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var weiXinExternalAuthSettings = _settingService.LoadSetting<WeiXinExternalAuthSettings>(storeScope);

            var model = new ConfigurationModel();
            model.AppId = weiXinExternalAuthSettings.AppId;
            model.AppSecret = weiXinExternalAuthSettings.AppSecret;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.AppIdOverrideForStore = _settingService.SettingExists(weiXinExternalAuthSettings, x => x.AppId, storeScope);
                model.AppSecretOverrideForStore = _settingService.SettingExists(weiXinExternalAuthSettings, x => x.AppSecret, storeScope);
            }

            return View("~/Plugins/ExternalAuth.WeiXin/Views/ExternalAuthWeiXin/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return Content("Access denied");

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var weiXinExternalAuthSettings = _settingService.LoadSetting<WeiXinExternalAuthSettings>(storeScope);

            //save settings
            weiXinExternalAuthSettings.AppId = model.AppId;
            weiXinExternalAuthSettings.AppSecret = model.AppSecret;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.AppIdOverrideForStore || storeScope == 0)
                _settingService.SaveSetting(weiXinExternalAuthSettings, x => x.AppId, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(weiXinExternalAuthSettings, x => x.AppId, storeScope);

            if (model.AppSecretOverrideForStore || storeScope == 0)
                _settingService.SaveSetting(weiXinExternalAuthSettings, x => x.AppSecret, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(weiXinExternalAuthSettings, x => x.AppSecret, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo()
        {
            return View("~/Plugins/ExternalAuth.WeiXin/Views/ExternalAuthWeiXin/PublicInfo.cshtml");
        }

        [NonAction]
        private ActionResult LoginInternal(string returnUrl, bool verifyResponse)
        {
            var processor = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName("ExternalAuth.WeiXin");
            if (processor == null ||
                !processor.IsMethodActive(_externalAuthenticationSettings) ||
                !processor.PluginDescriptor.Installed ||
                !_pluginFinder.AuthenticateStore(processor.PluginDescriptor, _storeContext.CurrentStore.Id))
                throw new NopException("WeiXin module cannot be loaded");

            var viewModel = new LoginModel();
            TryUpdateModel(viewModel);

            var result = _weiXinExternalProviderAuthorizer.Authorize(returnUrl, verifyResponse);
            switch (result.AuthenticationStatus)
            {
                case OpenAuthenticationStatus.Error:
                    {
                        if (!result.Success)
                            foreach (var error in result.Errors)
                                ExternalAuthorizerHelper.AddErrorsToDisplay(error);

                        return new RedirectResult(Url.LogOn(returnUrl));
                    }
                case OpenAuthenticationStatus.AssociateOnLogon:
                    {
                        return new RedirectResult(Url.LogOn(returnUrl));
                    }
                case OpenAuthenticationStatus.AutoRegisteredEmailValidation:
                    {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation });
                    }
                case OpenAuthenticationStatus.AutoRegisteredAdminApproval:
                    {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval });
                    }
                case OpenAuthenticationStatus.AutoRegisteredStandard:
                {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Standard });
                }
                    case OpenAuthenticationStatus.AutoRegisteredEmailEnter:
                {
                        var model = new RegisterModel();
                    model.ReturnUrl = returnUrl;
                        return View("~/Plugins/ExternalAuth.WeiXin/Views/ExternalAuthWeiXin/Register.cshtml", model);
                    }
                default:
                    break;
            }

            if (result.Result != null) return result.Result;
            return HttpContext.Request.IsAuthenticated ? new RedirectResult(!string.IsNullOrEmpty(returnUrl) ? returnUrl : "~/") : new RedirectResult(Url.LogOn(returnUrl));
        }

        public ActionResult Login(string returnUrl)
        {
            return LoginInternal(returnUrl, false);
        }


        [HttpGet]
        public ActionResult LoginCallback(string returnUrl)
        {
            return LoginInternal(returnUrl, true);
            
        }

        [HttpPost]
        public ActionResult LoginCallBack(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var processor = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName("ExternalAuth.WeiXin");
                if (processor == null ||
                    !processor.IsMethodActive(_externalAuthenticationSettings) ||
                    !processor.PluginDescriptor.Installed ||
                    !_pluginFinder.AuthenticateStore(processor.PluginDescriptor, _storeContext.CurrentStore.Id))
                    throw new NopException("WeiXin module cannot be loaded");

                var viewModel = new LoginModel();
                TryUpdateModel(viewModel);

                var result = _weiXinExternalProviderAuthorizer.RegisterEmail(model.ReturnUrl, model);

                switch (result.AuthenticationStatus)
                {
                    case OpenAuthenticationStatus.Error:
                        {
                            if (!result.Success)
                                foreach (var error in result.Errors)
                                    ExternalAuthorizerHelper.AddErrorsToDisplay(error);

                            return new RedirectResult(Url.LogOn(model.ReturnUrl));
                        }
                    case OpenAuthenticationStatus.AssociateOnLogon:
                        {
                            return new RedirectResult(Url.LogOn(model.ReturnUrl));
                        }
                    case OpenAuthenticationStatus.AutoRegisteredEmailValidation:
                        {
                            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation });
                        }
                    case OpenAuthenticationStatus.AutoRegisteredAdminApproval:
                        {
                            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval });
                        }
                    case OpenAuthenticationStatus.AutoRegisteredStandard:
                        {
                            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Standard });
                        }
                    case OpenAuthenticationStatus.AutoRegisteredEmailEnter:
                        {
                            var newmodel = new RegisterModel();
                            newmodel.ReturnUrl = model.ReturnUrl;
                            return View("~/Plugins/ExternalAuth.WeiXin/Views/ExternalAuthWeiXin/Register.cshtml", newmodel);
                        }
                    default:
                        break;
                }

                if (result.Result != null) return result.Result;
                return HttpContext.Request.IsAuthenticated ? new RedirectResult(!string.IsNullOrEmpty(model.ReturnUrl) ? model.ReturnUrl : "~/") : new RedirectResult(Url.LogOn(model.ReturnUrl));
            }
            return View("~/Plugins/ExternalAuth.WeiXin/Views/ExternalAuthWeiXin/Register.cshtml", model);
        }
    }
}
