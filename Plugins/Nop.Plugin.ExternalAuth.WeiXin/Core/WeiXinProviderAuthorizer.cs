using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.AspNet;
using Nop.Core;
using Nop.Core.Domain.Customers;
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
        private WeiXinClient _weiXinApplication;
#endregion

        public WeiXinProviderAuthorizer(IExternalAuthorizer authorizer,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            WeiXinExternalAuthSettings weiXinExternalAuthSettings,
            HttpContextBase httpContext,
            IWebHelper webHelper)
        {
            _authorizer = authorizer;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _weiXinExternalAuthSettings = weiXinExternalAuthSettings;
            _httpContext = httpContext;
            _webHelper = webHelper;
        }


        #region Utility


        private Uri GenerateLocalCallbackUri()
        {
            var t = _webHelper.GetStoreHost(true);
            var t2 = _webHelper.GetStoreHost(false);
            var t3 = _webHelper.GetStoreLocation();
            string url = Path.Combine(_webHelper.GetBaseUrl(),"plugins/externalauthWeiXin/logincallback");
            return new Uri(url);
        }

        



        private static readonly string[] UriRfc3986CharsToEscape = { "!", "*", "'", "(", ")" };

        internal static string EscapeUriDataStringRfc3986(string value)
        {
            var builder = new StringBuilder(Uri.EscapeDataString(value));
            for (int i = 0; i < UriRfc3986CharsToEscape.Length; i++)
            {
                builder.Replace(UriRfc3986CharsToEscape[i], Uri.HexEscape(UriRfc3986CharsToEscape[i][0]));
            }
            return builder.ToString();
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

        private WeiXinClient WeiXinApplication
        {
            get { return _weiXinApplication ?? (_weiXinApplication = new WeiXinClient(_weiXinExternalAuthSettings.AppId, _weiXinExternalAuthSettings.AppSecret)); }
        }

        private AuthorizeState VerifyAuthentication(string returnUrl)
        {

            var authResult = WeiXinApplication.VerifyAuthentication(_httpContext, GenerateLocalCallbackUri());

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
                };

                if (_externalAuthenticationSettings.AutoRegisterEnabled)
                    ParseClaims(authResult, parameters);

                var result = _authorizer.Authorize(parameters);

                return new AuthorizeState(returnUrl, result);
            }

            var state = new AuthorizeState(returnUrl, OpenAuthenticationStatus.Error);
            var error = authResult.Error != null ? authResult.Error.Message : "Unknown error";
            state.AddError(error);
            return state;
        }

        private void ParseClaims(AuthenticationResult authenticationResult, OAuthAuthenticationParameters parameters)
        {
            var claims = new UserClaims();
            claims.Contact = new ContactClaims();
            if (authenticationResult.ExtraData.ContainsKey("username"))
                claims.Contact.Email = authenticationResult.ExtraData["username"];
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
                throw new ArgumentException("Weixin plugin cannot automatically determine verifyResponse property");

            if (verifyResponse.Value)
                return VerifyAuthentication(returnUrl);

            return RequestAuthentication();
        }
    }
}
