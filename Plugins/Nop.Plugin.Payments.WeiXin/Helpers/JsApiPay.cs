using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using LitJson;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Payments.WeiXin.Models;

namespace Nop.Plugin.Payments.WeiXin.Helpers
{
    public class JsApiPay
    {
        private readonly WeiXinPaymentSettings _settings;
        private readonly string _redirectUri;

        /// <summary>
        /// openid用于调用统一下单接口
        /// </summary>
        public string Openid { get; set; }

        /// <summary>
        /// access_token用于获取收货地址js函数入口参数
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 商品金额，用于统一下单
        /// </summary>
        public string TotalFee { get; set; }

        /// <summary>
        /// 统一下单接口返回结果
        /// </summary>
        public WxPayData UnifiedOrderResult { get; set; }

        public JsApiPay(WeiXinPaymentSettings settings, string redirectUri)
        {
            _settings = settings;
            _redirectUri = redirectUri;
        }

        public void GetOpenidAndAccessToken()
        {
            if (!string.IsNullOrWhiteSpace(HttpContext.Current.Request.QueryString["code"]))
            {
                //获取code码，以获取openid和access_token
                var code = HttpContext.Current.Request.QueryString["code"];
                GetOpenidAndAccessTokenFromCode(code);
            }
            else
            {
                WxPayData data = new WxPayData();
                data.SetValue("appid", _settings.AppId);
                data.SetValue("redirect_uri", _redirectUri);
                data.SetValue("response_type", "code");
                data.SetValue("scope", "snsapi_base");
                data.SetValue("state", "STATE" + "#wechat_redirect");
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();

                try
                {
                    //触发微信返回code码    
                    HttpContext.Current.Response.Redirect(url);
                         
                    //Redirect函数会抛出ThreadAbortException异常，不用处理这个异常
                }
                catch (System.Threading.ThreadAbortException ex)
                {
                }
            }
        }


        /**
	    * 
	    * 通过code换取网页授权access_token和openid的返回数据，正确时返回的JSON数据包如下：
	    * {
	    *  "access_token":"ACCESS_TOKEN",
	    *  "expires_in":7200,
	    *  "refresh_token":"REFRESH_TOKEN",
	    *  "openid":"OPENID",
	    *  "scope":"SCOPE",
	    *  "unionid": "o6_bmasdasdsad6_2sgVt7hMZOPfL"
	    * }
	    * 其中access_token可用于获取共享收货地址
	    * openid是微信支付jsapi支付接口统一下单时必须的参数
        * 更详细的说明请参考网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
        * @失败时抛异常NopException
	    */
        public void GetOpenidAndAccessTokenFromCode(string code)
        {
            try
            {
                //构造获取openid及access_token的url
                WxPayData data = new WxPayData();
                data.SetValue("appid", _settings.AppId);
                data.SetValue("secret", _settings.AppSecret);
                data.SetValue("code", code);
                data.SetValue("grant_type", "authorization_code");
                string url = "https://api.weixin.qq.com/sns/oauth2/access_token?" + data.ToUrl();

                //请求url以获取数据
                string result = HttpUtil.Get(url);



                //保存access_token，用于收货地址获取
                JsonData jd = JsonMapper.ToObject(result);
                AccessToken = (string)jd["access_token"];

                //获取用户openid
                Openid = (string)jd["openid"];


            }
            catch (Exception ex)
            {

                throw new NopException(ex.ToString());
            }
        }

        /**
         * 调用统一下单，获得下单结果
         * @return 统一下单结果
         * @失败时抛异常NopException
         */
        public WxPayData GetUnifiedOrderResult(int orderId, string clientId, string notifyUrl)
        {
            //统一下单
            var data = new WxPayData();
            data.SetValue("body", "test");
            data.SetValue("attach", "test");
            data.SetValue("out_trade_no", orderId);
            data.SetValue("total_fee", TotalFee);
            data.SetValue("time_start", DateTime.Now.AddHours(8).ToString("yyyyMMddHHmmss"));
            data.SetValue("time_expire", DateTime.Now.AddHours(10).ToString("yyyyMMddHHmmss"));
            data.SetValue("goods_tag", "test");
            data.SetValue("trade_type", "JSAPI");
            data.SetValue("openid", Openid);

            WxPayData result = WeiXinHelper.UnifiedOrder(data, clientId, notifyUrl, _settings);
            if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
            {
                throw new NopException("UnifiedOrder response error!");
            }

            UnifiedOrderResult = result;
            return result;
        }



        /**
        *  
        * 从统一下单成功返回的数据中获取微信浏览器调起jsapi支付所需的参数，
        * 微信浏览器调起JSAPI时的输入参数格式如下：
        * {
        *   "appId" : "wx2421b1c4370ec43b",     //公众号名称，由商户传入     
        *   "timeStamp":" 1395712654",         //时间戳，自1970年以来的秒数     
        *   "nonceStr" : "e61463f8efa94090b1f366cccfbbb444", //随机串     
        *   "package" : "prepay_id=u802345jgfjsdfgsdg888",     
        *   "signType" : "MD5",         //微信签名方式:    
        *   "paySign" : "70EA570631E4BB79628FBCA90534C63FF7FADD89" //微信签名 
        * }
        * @return string 微信浏览器调起JSAPI时的输入参数，json格式可以直接做参数用
        * 更详细的说明请参考网页端调起支付API：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_7
        * 
        */
        public string GetJsApiParameters()
        {
            WxPayData jsApiParam = new WxPayData();
            jsApiParam.SetValue("appId", UnifiedOrderResult.GetValue("appid"));
            jsApiParam.SetValue("timeStamp", WeiXinHelper.GenerateTimeStamp());
            jsApiParam.SetValue("nonceStr", Guid.NewGuid().ToString("N"));
            jsApiParam.SetValue("package", "prepay_id=" + UnifiedOrderResult.GetValue("prepay_id"));
            jsApiParam.SetValue("signType", "MD5");
            jsApiParam.SetValue("paySign", jsApiParam.MakeSign(_settings.AppSecret));

            string parameters = jsApiParam.ToJson();
            return parameters;
        }


        /**
	    * 
	    * 获取收货地址js函数入口参数,详情请参考收货地址共享接口：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_9
	    * @return string 共享收货地址js函数需要的参数，json格式可以直接做参数使用
	    */
        public string GetEditAddressParameters()
        {
            string parameter = "";
            try
            {
                string host = HttpContext.Current.Request.Url.Host;
                string path = HttpContext.Current.Request.Path;
                string queryString = HttpContext.Current.Request.Url.Query;
                //这个地方要注意，参与签名的是网页授权获取用户信息时微信后台回传的完整url
                string url = "http://" + host + path + queryString;

                //构造需要用SHA1算法加密的数据
                WxPayData signData = new WxPayData();
                signData.SetValue("appid", _settings.AppId);
                signData.SetValue("url", url);
                signData.SetValue("timestamp", WeiXinHelper.GenerateTimeStamp());
                signData.SetValue("noncestr", Guid.NewGuid().ToString("N"));
                signData.SetValue("accesstoken", AccessToken);
                string param = signData.ToUrl();


                //SHA1加密
                string addrSign = FormsAuthentication.HashPasswordForStoringInConfigFile(param, "SHA1");

                //获取收货地址js函数入口参数
                var afterData = new WxPayData();
                afterData.SetValue("appId", _settings.AppId);
                afterData.SetValue("scope", "jsapi_address");
                afterData.SetValue("signType", "sha1");
                afterData.SetValue("addrSign", addrSign);
                afterData.SetValue("timeStamp", signData.GetValue("timestamp"));
                afterData.SetValue("nonceStr", signData.GetValue("noncestr"));

                //转为json格式
                parameter = afterData.ToJson();
            }
            catch (Exception ex)
            {
                throw new NopException(ex.ToString());
            }

            return parameter;
        }
    }
}
