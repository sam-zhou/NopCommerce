using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using DotNetOpenAuth.AspNet;
using Newtonsoft.Json.Linq;
using Nop.Core;

namespace Lynex.Weixin.Service
{
    public class WeiXinClient
    {
        #region Constants and Fields

        /// <summary>
        /// The provider name.
        /// </summary>
        
        private const string AuthorizationEndpoint = "https://open.weixin.qq.com/connect/oauth2/authorize";

        private const string WebAuthorizationEndpoint = "https://open.weixin.qq.com/connect/qrconnect";

        private const string TokenEndpoint = "https://api.weixin.qq.com/sns/oauth2/access_token";

        private const string UserInfoEndpoint = "https://api.weixin.qq.com/sns/userinfo";

        private const string Scope = "snsapi_userinfo";// "snsapi_base" : "snsapi_userinfo"

        private readonly string _appId;

        private readonly string _appSecret;

        private readonly string _providerName;

        #endregion

        #region Constructors and Destructors

        public WeiXinClient(string appId, string appSecret)
        {
            _providerName = "weixin";
            _appId = appId;
            _appSecret = appSecret;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the name of the provider which provides authentication service.
        /// </summary>
        public string ProviderName
        {
            get
            {
                return this._providerName;
            }
        }

        #endregion

        #region Public Methods and Operators

        public AuthenticationResult VerifyCode(HttpContextBase context, Uri returnPageUrl)
        {
            string code = context.Request.QueryString["code"];
            if (string.IsNullOrEmpty(code))
            {
                return AuthenticationResult.Failed;
            }
            var extraData = new Dictionary<string, string>();
            extraData.Add("code", code);
            var result =  new AuthenticationResult(true, "weixin", null, null, extraData);


            return result;
        }

        public AuthenticationResult VerifyAuthentication(Uri returnUrl, string code)
        {
            var tokenObject = QueryAccessToken(returnUrl, code);
            if (tokenObject["access_token"] == null || tokenObject["openid"] == null || tokenObject["refresh_token"] == null)
            {
                return AuthenticationResult.Failed;
            }

            var accessToken = (string)tokenObject["access_token"];
            var openid = (string)tokenObject["openid"];
            var refreshToken = (string)tokenObject["refresh_token"];

            IDictionary<string, string> userData = GetUserData(accessToken, openid);
            if (userData == null)
            {
                return AuthenticationResult.Failed;
            }

            string name;

            // Some oAuth providers do not return value for the 'username' attribute. 
            // In that case, try the 'name' attribute. If it's still unavailable, fall back to 'id'
            if (!userData.TryGetValue("username", out name) && !userData.TryGetValue("name", out name))
            {
                name = openid;
            }

            // add the access token to the user data dictionary just in case page developers want to use it
            userData["accesstoken"] = accessToken;
            userData["refreshtoken"] = refreshToken;
            var result = new AuthenticationResult(true, ProviderName, openid, name, userData);
            return result;
        }

        #endregion

        public static Uri GenerateCodeRequestUrl(string appId, string callbackUrl)
        {
            var builder = new UriBuilder(AuthorizationEndpoint);
            var args = new Dictionary<string, string>();
            args.Add("appid", appId);
            args.Add("redirect_uri", callbackUrl);
            args.Add("response_type", "code");
            args.Add("scope", Scope);
            args.Add("state", "STATE#wechat_redirect");
            AppendQueryArgs(builder, args);
            return builder.Uri;
        }

        public static Uri GenerateWebLoginRequestUrl(string appId, string callbackUrl)
        {
            var builder = new UriBuilder(WebAuthorizationEndpoint);
            var args = new Dictionary<string, string>();
            args.Add("appid", appId);
            args.Add("redirect_uri", callbackUrl);
            args.Add("response_type", "code");
            args.Add("scope", "snsapi_login");
            args.Add("state", "STATE#wechat_redirect");
            AppendQueryArgs(builder, args);
            return builder.Uri;
        }

        internal static void AppendQueryArgs(UriBuilder builder, Dictionary<string, string> args)
        {
            if ((args != null) && (args.Any()))
            {
                var builder2 = new StringBuilder(50 + (args.Count() * 10));
                if (!string.IsNullOrEmpty(builder.Query))
                {
                    builder2.Append(builder.Query.Substring(1));
                    builder2.Append('&');
                }
                builder2.Append(CreateQueryString(args));
                builder.Query = builder2.ToString();
            }
        }

        internal static string CreateQueryString(Dictionary<string, string> args)
        {
            if (!args.Any())
            {
                return string.Empty;
            }
            var builder = new StringBuilder(args.Count() * 10);
            foreach (var pair in args)
            {
                builder.Append(pair.Key);
                builder.Append('=');
                builder.Append(pair.Value);

                builder.Append('&');
            }
            builder.Length--;
            return builder.ToString();
        }

        private static readonly string[] UriRfc3986CharsToEscape = { "!", "*", "'", "(", ")" };

        private string EscapeUriDataStringRfc3986(string value)
        {
            var builder = new StringBuilder(Uri.EscapeDataString(value));
            for (int i = 0; i < UriRfc3986CharsToEscape.Length; i++)
            {
                builder.Replace(UriRfc3986CharsToEscape[i], Uri.HexEscape(UriRfc3986CharsToEscape[i][0]));
            }
            return builder.ToString();
        }

        protected IDictionary<string, string> GetUserData(string accessToken, string openId)
        {
            var userData = new Dictionary<string, string>();
            using (var client = new System.Net.WebClient())
            {
                using (var stream = client.OpenRead(UserInfoEndpoint + "?access_token=" + EscapeUriDataStringRfc3986(accessToken) + "&openid=" + openId + "&lang=zh_CN"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        JObject jObject = JObject.Parse(reader.ReadToEnd());
                        userData.Add("id", (string)jObject["openid"]);
                        userData.Add("unionid", (string)jObject["unionid"]);
                        userData.Add("picture", (string)jObject["headimgurl"]);
                        userData.Add("username", "wx" + (string)jObject["unionid"]);
                        userData.Add("name", (string)jObject["nickname"]);
                    }
                }
            }

            return userData;
        }

        public void RequestAuthentication(HttpContextBase context, Uri returnUrl)
        {
            context.Response.Redirect(returnUrl.AbsoluteUri, endResponse: true);
        }

        protected JObject QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            var args = new Dictionary<string, string>();
            args.Add("appid", _appId);
            args.Add("secret", _appSecret);
            args.Add("code", authorizationCode);
            args.Add("grant_type", "authorization_code");
            string query = "?" + CreateQueryString(args);
            var data = Get(TokenEndpoint + query);
            if (string.IsNullOrEmpty(data) || !data.Contains("access_token"))
                return null;
            return JObject.Parse(data);
        }

        public string Get(string url)
        {
            GC.Collect();
            string result = "";
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                ServicePointManager.DefaultConnectionLimit = 200;
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd().Trim();
                sr.Close();
            }
            catch (System.Threading.ThreadAbortException e)
            {
                System.Threading.Thread.ResetAbort();
            }
            catch (WebException e)
            {
                throw new NopException(e.ToString());
            }
            catch (Exception e)
            {
                throw new NopException(e.ToString());
            }
            finally
            {

                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
            }
            return result;
        }
    }
}
