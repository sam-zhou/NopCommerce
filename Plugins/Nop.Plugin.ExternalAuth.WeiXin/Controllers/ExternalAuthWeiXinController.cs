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
using Nop.Services.Customers;

namespace Nop.Plugin.ExternalAuth.WeiXin.Controllers
{
    public class ExternalAuthWeiXinController : BasePluginController
    {
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IWeiXinExternalProviderAuthorizer _weiXinExternalProviderAuthorizer;

        public ExternalAuthWeiXinController(ISettingService settingService,
            IPermissionService permissionService, IStoreContext storeContext,
            IStoreService storeServie, IWorkContext workContext, ICustomerService customerService,
            ILocalizationService localizationService, IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings, IPluginFinder pluginFinder,IWeiXinExternalProviderAuthorizer weiXinExternalProviderAuthorizer)
        {
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._storeContext = storeContext;
            this._storeService = storeServie;
            this._workContext = workContext;
            _customerService = customerService;
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
            model.WebAppId = weiXinExternalAuthSettings.WebAppId;
            model.WebAppSecret = weiXinExternalAuthSettings.WebAppSecret;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.AppId_OverrideForStore = _settingService.SettingExists(weiXinExternalAuthSettings, x => x.AppId, storeScope);
                model.AppSecret_OverrideForStore = _settingService.SettingExists(weiXinExternalAuthSettings, x => x.AppSecret, storeScope);
                model.WebAppId_OverrideForStore = _settingService.SettingExists(weiXinExternalAuthSettings, x => x.WebAppId, storeScope);
                model.WebAppSecret_OverrideForStore = _settingService.SettingExists(weiXinExternalAuthSettings, x => x.WebAppSecret, storeScope);
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
            weiXinExternalAuthSettings.WebAppId = model.WebAppId;
            weiXinExternalAuthSettings.WebAppSecret = model.WebAppSecret;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.AppId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(weiXinExternalAuthSettings, x => x.AppId, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(weiXinExternalAuthSettings, x => x.AppId, storeScope);

            if (model.AppSecret_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(weiXinExternalAuthSettings, x => x.AppSecret, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(weiXinExternalAuthSettings, x => x.AppSecret, storeScope);

            if (model.WebAppId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(weiXinExternalAuthSettings, x => x.WebAppId, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(weiXinExternalAuthSettings, x => x.WebAppId, storeScope);

            if (model.WebAppSecret_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(weiXinExternalAuthSettings, x => x.WebAppSecret, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(weiXinExternalAuthSettings, x => x.WebAppSecret, storeScope);

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
        private ActionResult LoginInternal(string returnUrl, bool? verifyResponse)
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

        public ActionResult WebLogin(string returnUrl)
        {
            return LoginInternal(returnUrl, null);
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


                var found = _customerService.GetCustomerByEmail(model.Email);
                if (found != null)
                {
                    ModelState.AddModelError("Email","电子邮箱已被注册");
                    return View("~/Plugins/ExternalAuth.WeiXin/Views/ExternalAuthWeiXin/Register.cshtml", model);
                }


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
