using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.AspNet;
using Lynex.Weixin.Service;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Plugin.ExternalAuth.WeiXin.Models;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Infrastructure;

namespace Nop.Plugin.ExternalAuth.WeiXin.Core
{
    public class WeiXinProviderAuthorizer : IWeiXinExternalProviderAuthorizer
    {
#region properties
        private readonly IExternalAuthorizer _authorizer;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly WeiXinExternalAuthSettings _weiXinExternalAuthSettings;
        private readonly HttpContextBase _httpContext;
        private readonly IWebHelper _webHelper;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private WeiXinClient _weiXinApplication;
#endregion

        public WeiXinProviderAuthorizer(IExternalAuthorizer authorizer,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            WeiXinExternalAuthSettings weiXinExternalAuthSettings,
            HttpContextBase httpContext,
            IWebHelper webHelper,
            IOpenAuthenticationService openAuthenticationService)
        {
            _authorizer = authorizer;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _weiXinExternalAuthSettings = weiXinExternalAuthSettings;
            _httpContext = httpContext;
            _webHelper = webHelper;
            _openAuthenticationService = openAuthenticationService;
        }


        #region Utility



        private HttpSessionStateBase Session
        {
            get
            {
                return EngineContext.Current.Resolve<HttpSessionStateBase>();
            }
        }

        private Uri GenerateLocalCallbackUri()
        {
            string url = Path.Combine(_webHelper.GetBaseUrl(),"plugins/externalauthWeiXin/logincallback");
            return new Uri(url);
        }

        internal static string NormalizeHexEncoding(string url)
        {
            var chars = url.ToCharArray();
            for (int i = 0; i < chars.Length - 2; i++)
            {
                if (chars[i] == '%')
                {
                    chars[i + 1] = char.ToUpperInvariant(chars[i + 1]);
                    chars[i + 2] = char.ToUpperInvariant(chars[i + 2]);
                    i += 2;
                }
            }
            return new string(chars);
        }

        #endregion

        private AuthorizeState RequestAuthentication()
        {
            var authUrl = WeiXinClient.GenerateCodeRequestUrl(_weiXinExternalAuthSettings.AppId, GenerateLocalCallbackUri().AbsoluteUri).AbsoluteUri;
            return new AuthorizeState("", OpenAuthenticationStatus.RequiresRedirect) { Result = new RedirectResult(authUrl) };
        }

        private AuthorizeState WebAppRequestAuthentication()
        {
            var authUrl = WeiXinClient.GenerateWebLoginRequestUrl(_weiXinExternalAuthSettings.AppId, GenerateLocalCallbackUri().AbsoluteUri).AbsoluteUri;
            return new AuthorizeState("", OpenAuthenticationStatus.RequiresRedirect) { Result = new RedirectResult(authUrl) };
        }

        private WeiXinClient WeiXinApplication
        {
            get { return _weiXinApplication ?? (_weiXinApplication = new WeiXinClient(_weiXinExternalAuthSettings.AppId, _weiXinExternalAuthSettings.AppSecret)); }
        }

        private AuthorizeState VerifyCode(string returnUrl)
        {
            var authResult = WeiXinApplication.VerifyCode(_httpContext, GenerateLocalCallbackUri());

            if (authResult.IsSuccessful)
            {
                if (!authResult.ExtraData.ContainsKey("code"))
                {
                    throw new Exception("Authentication code does not contain id data");
                }
                var code = authResult.ExtraData["code"];

                authResult = WeiXinApplication.VerifyAuthentication(GenerateLocalCallbackUri(), code);

                if (authResult.IsSuccessful)
                {
                    if (!authResult.ExtraData.ContainsKey("id"))
                        throw new Exception("Authentication result does not contain id data");

                    if (!authResult.ExtraData.ContainsKey("accesstoken"))
                        throw new Exception("Authentication result does not contain accesstoken data");

                    var parameters = new OAuthAuthenticationParameters(Provider.SystemName)
                    {
                        ExternalIdentifier = authResult.ProviderUserId,
                        OAuthToken = authResult.ExtraData["accesstoken"],
                        OAuthAccessToken = authResult.ExtraData["refreshtoken"],
                        ExternalDisplayIdentifier = returnUrl
                    };

                    if (_externalAuthenticationSettings.AutoRegisterEnabled)
                        ParseClaims(authResult, parameters, new RegisterModel());

                    var user = _openAuthenticationService.GetUser(parameters);

                    //Login User
                    if (user != null)
                    {
                        var result = _authorizer.Authorize(parameters);
                        return new AuthorizeState(returnUrl, result);
                    }// Register User
                    else
                    {
                        SaveOAuthParametersToSession(parameters);
                        return new AuthorizeState("/Plugins/ExternalAuthWeiXin/Register", OpenAuthenticationStatus.AutoRegisteredEmailEnter);
                    }
                }

                
            }

            var state = new AuthorizeState(returnUrl, OpenAuthenticationStatus.Error);
            var error = authResult.Error != null ? authResult.Error.Message : "Unknown error";
            state.AddError(error);
            return state;
        }

        public AuthorizeState RegisterEmail(string returnUrl, RegisterModel model)
        {
            var parameters = GetOAuthParametersFromSession();
            
            if (parameters == null || parameters.UserClaims == null || parameters.ProviderSystemName != "ExternalAuth.WeiXin")
            {
                throw new Exception("Authentication request does not contain required data");
            }

            var claim = parameters.UserClaims.FirstOrDefault();
            if (claim != null)
            {
                claim.Contact.Email = model.Email;

                claim.Password = new PasswordClaims();
                claim.Password.Password = model.Password;
                claim.Password.ConfirmPassword = model.ConfirmPassword;
            }

            

            var result = _authorizer.Authorize(parameters);
            return new AuthorizeState(returnUrl, result);

        }

        

        private void SaveOAuthParametersToSession(OAuthAuthenticationParameters parameters)
        {

            Session["nop.externalauth.weixin.parameters"] = parameters;
        }

        private OAuthAuthenticationParameters GetOAuthParametersFromSession()
        {
            var parameters = Session["nop.externalauth.weixin.parameters"];
            if (parameters != null)
            {
                Session.Remove("nop.externalauth.weixin.parameters");
            }

            return parameters as OAuthAuthenticationParameters;
        }

        //public AuthorizeState RegisterEmail(string email, string password, string confirmpassword)
        //{
        //    var parameters = GetOAuthAuthenticationParametersFromSession();
        //    if (parameters != null)
        //    {
        //        var claim = parameters.UserClaims.FirstOrDefault();

        //        if (claim != null)
        //        {
        //            claim.Contact = new ContactClaims();
        //            claim.Contact.Email = email;
                    

        //            var result = _authorizer.Authorize(parameters);

        //            return new AuthorizeState(parameters.ExternalDisplayIdentifier, result);
        //        }


        //    }

        //    var state = new AuthorizeState("Login", OpenAuthenticationStatus.Error);
        //    state.AddError("Unknown error");
        //    return state;

        //}

        private void ParseClaims(AuthenticationResult authenticationResult, OAuthAuthenticationParameters parameters, RegisterModel model)
        {
            var claims = new UserClaims();
            claims.Contact = new ContactClaims();
            claims.Contact.Email = model.Email;
            claims.Password = new PasswordClaims();
            claims.Password.Password = model.Password;
            claims.Password.ConfirmPassword = model.ConfirmPassword;

            claims.Name = new NameClaims();
            if (authenticationResult.ExtraData.ContainsKey("name"))
            {
                var name = authenticationResult.ExtraData["name"];
                if (!String.IsNullOrEmpty(name))
                {
                    var nameSplit = name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (nameSplit.Length >= 2)
                    {
                        claims.Name.First = nameSplit[0];
                        claims.Name.Last = nameSplit[1];
                    }
                    else
                    {
                        claims.Name.Last = nameSplit[0];
                    }
                }
            }

            if (authenticationResult.ExtraData.ContainsKey("picture"))
            {
                claims.Media = new MediaClaims();
                claims.Media.Images = new ImageClaims();
                claims.Media.Images.Default = authenticationResult.ExtraData["picture"];
            }

            parameters.AddClaim(claims);
        }

        public AuthorizeState Authorize(string returnUrl, bool? verifyResponse = null)
        {
            if (!verifyResponse.HasValue)
                return WebAppRequestAuthentication();

            if (verifyResponse.Value)
                return VerifyCode(returnUrl);

            return RequestAuthentication();
        }
    }
}
