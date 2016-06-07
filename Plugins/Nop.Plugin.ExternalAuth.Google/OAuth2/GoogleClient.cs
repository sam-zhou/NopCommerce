using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json.Linq;
using Nop.Plugin.ExternalAuth.Google.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;

namespace Nop.Plugin.ExternalAuth.Google.OAuth2
{
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Google", Justification = "Brand name")]
    public class GoogleClient : OAuth2Client
    {
        private const string _authorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";

        private const string _tokenEndpoint = "https://accounts.google.com/o/oauth2/token";

        private const string _userInfoEndpoint = "https://www.googleapis.com/oauth2/v1/userinfo";

        private readonly string _appId;

        private readonly string _appSecret;

        private readonly string[] _scopes;

        public GoogleClient(string appId, string appSecret)
            : this(
                  appId, 
                  appSecret, "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email")
        {  }
 
       
        public GoogleClient(string appId, string appSecret, params string[] scope)
            : base("google") {
          
            this._appId = appId;
            this._appSecret = appSecret;
            this._scopes = scope;
        }

        private string HttpPost(string URI, string Parameters)
        {
            WebRequest req = WebRequest.Create(URI);
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(Parameters);
            req.ContentLength = bytes.Length;
            using (var stream = req.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            var res = (HttpWebResponse)req.GetResponse();
            using (var stream = res.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd().Trim();
                }
            }           
        }           

        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            var builder = new UriBuilder(_authorizationEndpoint);
            var args = new Dictionary<string, string>();
            args.Add("response_type", "code");
            args.Add("client_id", _appId);
            args.Add("redirect_uri", GoogleProviderAuthorizer.NormalizeHexEncoding(returnUrl.AbsoluteUri));
            args.Add("scope", string.Join(" ", this._scopes));
            GoogleProviderAuthorizer.AppendQueryArgs(builder, args);
            return builder.Uri;
        }

        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            var userData = new Dictionary<string, string>();
            using (WebClient client = new WebClient())
            {
                using (Stream stream = client.OpenRead(_userInfoEndpoint + "?access_token=" + GoogleProviderAuthorizer.EscapeUriDataStringRfc3986(accessToken)))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        JObject jObject = JObject.Parse(reader.ReadToEnd());
                        userData.Add("id", (string)jObject["id"]);
                        userData.Add("username", (string)jObject["email"]);
                        userData.Add("name", (string)jObject["given_name"] + " " + (string)jObject["family_name"]);
                    }
                }
            }

            return userData;
        }

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            var args = new Dictionary<string, string>();
            args.Add("response_type", "code");
            args.Add("code", authorizationCode);
            args.Add("client_id", _appId);
            args.Add("client_secret", _appSecret);
            args.Add("redirect_uri", GoogleProviderAuthorizer.NormalizeHexEncoding(returnUrl.AbsoluteUri));
            args.Add("grant_type", "authorization_code");
            string query = "?" + GoogleProviderAuthorizer.CreateQueryString(args);
            string data = HttpPost(_tokenEndpoint, query);
            if (string.IsNullOrEmpty(data))
                return null;
            JObject jObject = JObject.Parse(data);
            return (string)jObject["access_token"];                  
        }
    }
}
