using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;

namespace Nop.Plugin.ExternalAuth.WeiXin.Core
{
    public class WeiXinClient : OAuth2Client
    {
        private const string AuthorizationEndpoint = "https://open.weixin.qq.com/connect/qrconnect";

        private const string TokenEndpoint = "https://api.weixin.qq.com/sns/oauth2/access_token";

        private const string UserInfoEndpoint = "https://api.weixin.qq.com/sns/userinfo";

        private readonly string _appId;

        private readonly string _appSecret;

        private readonly string[] _scopes;

        public WeiXinClient(string appId, string appSecret)
            : this(
                  appId, 
                  appSecret, "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email")
        {  }


        public WeiXinClient(string appId, string appSecret, params string[] scope)
            : base("weixin") {
          
            _appId = appId;
            _appSecret = appSecret;
            _scopes = scope;
        }

        private string HttpPost(string uri, string parameters)
        {
            var req = WebRequest.Create(uri);
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            var bytes = Encoding.ASCII.GetBytes(parameters);
            req.ContentLength = bytes.Length;
            using (var stream = req.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            var res = (HttpWebResponse)req.GetResponse();
            using (var stream = res.GetResponseStream())
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd().Trim();
                    }
                }
                else
                {
                    return null;
                }
            }           
        }           

        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            var builder = new UriBuilder(AuthorizationEndpoint);
            var args = new Dictionary<string, string>();
            args.Add("response_type", "code");
            args.Add("client_id", _appId);
            args.Add("redirect_uri", WeiXinProviderAuthorizer.NormalizeHexEncoding(returnUrl.AbsoluteUri));
            args.Add("scope", string.Join(" ", this._scopes));
            WeiXinProviderAuthorizer.AppendQueryArgs(builder, args);
            return builder.Uri;
        }

        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            var userData = new Dictionary<string, string>();
            using (var client = new WebClient())
            {
                using (var stream = client.OpenRead(UserInfoEndpoint + "?access_token=" + WeiXinProviderAuthorizer.EscapeUriDataStringRfc3986(accessToken)))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        JObject jObject = JObject.Parse(reader.ReadToEnd());
                        userData.Add("id", (string)jObject["openid"]);
                        //userData.Add("username", (string)jObject["email"]);
                        userData.Add("name", (string)jObject["nickname"]);
                    }
                }
            }

            return userData;
        }

        //protected override IDictionary<string, string> GetUserData(string accessToken)
        //{
        //    var userData = new Dictionary<string, string>();
        //    using (var client = new WebClient())
        //    {
        //        using (var stream = client.OpenRead(UserInfoEndpoint + "?access_token=" + WeiXinProviderAuthorizer.EscapeUriDataStringRfc3986(accessToken)))
        //        {
        //            using (var reader = new StreamReader(stream))
        //            {
        //                JObject jObject = JObject.Parse(reader.ReadToEnd());
        //                userData.Add("id", (string)jObject["id"]);
        //                userData.Add("username", (string)jObject["email"]);
        //                userData.Add("name", (string)jObject["given_name"] + " " + (string)jObject["family_name"]);
        //            }
        //        }
        //    }

        //    return userData;
        //}

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            var args = new Dictionary<string, string>();
            args.Add("response_type", "code");
            args.Add("code", authorizationCode);
            args.Add("client_id", _appId);
            args.Add("client_secret", _appSecret);
            args.Add("redirect_uri", WeiXinProviderAuthorizer.NormalizeHexEncoding(returnUrl.AbsoluteUri));
            args.Add("grant_type", "authorization_code");
            string query = "?" + WeiXinProviderAuthorizer.CreateQueryString(args);
            string data = HttpPost(TokenEndpoint, query);
            if (string.IsNullOrEmpty(data))
                return null;
            JObject jObject = JObject.Parse(data);
            return (string)jObject["access_token"];                  
        }
    }
}
