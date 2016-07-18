﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using LitJson;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Extension;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.WeiXin.Controllers;
using Nop.Plugin.Payments.WeiXin.Helpers;
using Nop.Plugin.Payments.WeiXin.Models;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Web.Framework;
using ZXing;
using ZXing.Common;
using Brushes = System.Windows.Media.Brushes;


namespace Nop.Plugin.Payments.WeiXin
{
    /// <summary>
    /// WeiXin payment processor
    /// </summary>
    public class WeiXinPaymentProcessor : BasePlugin, IPaymentMethod, IConsumer<OrderPaidEvent>, IConsumer<ShipmentSentEvent>
    {
        #region Fields

        private readonly WeiXinPaymentSettings _weiXinPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly HttpContextBase _httpContext;
        private readonly ILogger _log;
        private readonly string _paidTemplateId = "qnKBZRpyvXEBEDEYR8sJAGNW-rfRhMKbHVF1tdRPQdI";
        private readonly string _shippedTemplateId = "JDkdWbv2RfISnw_0jBfXSfR6na_74noXd4bTHKIlhbA";
        private readonly string _cancelledTemplateId = "3hzGSYrsrFeSDWtWdZmauez6mHjHsA9RvzOjQlDvFew";
        private readonly string _shippingExceptionTemplateId = "-tVYG3Kt6Bjhu7PxA0SC5lpTWBIWXP6ABV0khnFDpu8";
        private readonly string _orderReceivedTemplateId = "suwFXWVi9Vk59vOyycLAGmJZaZiwRk53A8pOjl90RZI";
        private readonly string _customExceptionTemplateId = "RHRHMYsoARcvaPHW6E4bmyjD89QhMUN7pP1CosTeP78";
        private const string OrderUrl = @"https://api.mch.weixin.qq.com/pay/unifiedorder";
        private readonly string _notifyUrl;
        #endregion

        #region Ctor

        public WeiXinPaymentProcessor(WeiXinPaymentSettings weiXinPaymentSettings,
            ISettingService settingService, IWebHelper webHelper,
            IStoreContext storeContext, IWorkContext workContext, HttpContextBase httpContext, ILogger log)
        {
            this._weiXinPaymentSettings = weiXinPaymentSettings;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
            _workContext = workContext;
            _httpContext = httpContext;
            _log = log;

            _notifyUrl = Path.Combine(_webHelper.GetStoreLocation(), "Plugins/PaymentWeiXin/Notify");
        }

        #endregion

        #region Utilities
        private string ParseXML(Hashtable parameters)
        {
            var sb = new StringBuilder();
            sb.Append("<xml>");
            var akeys = new ArrayList(parameters.Keys);
            foreach (string k in akeys)
            {
                var v = (string)parameters[k];
                if (Regex.IsMatch(v, @"^[0-9.]$"))
                {
                    sb.Append("<" + k + ">" + v + "</" + k + ">");
                }
                else
                {
                    sb.Append("<" + k + "><![CDATA[" + v + "]]></" + k + ">");
                }
            }
            sb.Append("</xml>");

            var utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(sb.ToString());
            var result = utf8.GetString(utfBytes, 0, utfBytes.Length);

            return result;
        }

        private string CreatUrlParameters(Hashtable parameters)
        {
            var output = string.Empty;
            var akeys = new ArrayList(parameters.Keys);
            foreach (var k in akeys)
            {
                var v = (string)parameters[k];
                if (output == string.Empty)
                {
                    output += "?";
                }
                else
                {
                    output += "&";
                }

                output += (k + "=" + v);
            }
            return output;
        }

        private string CreateMd5Sign(string key, string value, Hashtable parameters, string contentEncoding)
        {
            var sb = new StringBuilder();
            var akeys = new ArrayList(parameters.Keys);
            akeys.Sort();
            foreach (string k in akeys)
            {
                var v = (string)parameters[k];
                if (null != v && "".CompareTo(v) != 0
                    && "sign".CompareTo(k) != 0 && "key".CompareTo(k) != 0)
                {
                    sb.Append(k + "=" + v + "&");
                }
            }
            sb.Append(key + "=" + value);
            string sign = GetMD5(sb.ToString(), contentEncoding).ToUpper();
            return sign;
        }
        /// <summary>
        /// Gets MD5 hash
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="inputCharset">Input charset</param>
        /// <returns>Result</returns>
        public string GetMD5(string input, string inputCharset)
        {
            var md5 = new MD5CryptoServiceProvider();
            var t = md5.ComputeHash(Encoding.GetEncoding(inputCharset).GetBytes(input));
            var sb = new StringBuilder(32);
            for (var i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets HTTP
        /// </summary>
        /// <param name="strUrl">Url</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Result</returns>
        public string Get_Http(string strUrl, int timeout)
        {
            string strResult;
            try
            {
                HttpWebRequest myReq = (HttpWebRequest)HttpWebRequest.Create(strUrl);
                myReq.Timeout = timeout;
                HttpWebResponse HttpWResp = (HttpWebResponse)myReq.GetResponse();
                Stream myStream = HttpWResp.GetResponseStream();
                StreamReader sr = new StreamReader(myStream, Encoding.Default);
                StringBuilder strBuilder = new StringBuilder();
                while (-1 != sr.Peek())
                {
                    strBuilder.Append(sr.ReadLine());
                }

                strResult = strBuilder.ToString();
            }
            catch (Exception exc)
            {
                strResult = "Error: " + exc.Message;
            }
            return strResult;
        }

        public string GetQrCode(string url)
        {
            var qrWriter = new BarcodeWriter();
            qrWriter.Format = BarcodeFormat.QR_CODE;
            qrWriter.Options = new EncodingOptions { Height = 200, Width = 200, Margin = 5 };

            using (var q = qrWriter.Write(url))
            {
                using (var ms = new MemoryStream())
                {
                    q.Save(ms, ImageFormat.Png);
                    return String.Format("data:image/png;base64,{0}", Convert.ToBase64String(ms.ToArray()));
                }
            }
        }

        public string GetUrlForMethodOne(int orderId)
        {
            var url = @"weixin://wxpay/bizpayurl";
            var packageParameter = new Hashtable();
            packageParameter.Add("appid", _weiXinPaymentSettings.AppId);
            packageParameter.Add("mch_id", _weiXinPaymentSettings.MchId);
            packageParameter.Add("time_stamp", CommonExtension.GetCurrentTimeStamp().ToString());
            packageParameter.Add("nonce_str", Guid.NewGuid().ToString("N"));
            packageParameter.Add("product_id", orderId.ToString());
            var sign = CreateMd5Sign("key", _weiXinPaymentSettings.AppSecret, packageParameter, "utf-8");
            packageParameter.Add("sign", sign);
            url += CreatUrlParameters(packageParameter);

            return url;
        }

        public string Unifiedorder(string productId, string body, string detail, string orderId, string total)
        {
            var packageParameter = new Hashtable();
            packageParameter.Add("appid", _weiXinPaymentSettings.AppId);
            packageParameter.Add("mch_id", _weiXinPaymentSettings.MchId);

            packageParameter.Add("nonce_str", Guid.NewGuid().ToString("N"));
            packageParameter.Add("product_id", productId);
            packageParameter.Add("body", body);
            
            packageParameter.Add("detail", detail);
            packageParameter.Add("out_trade_no", orderId);
            packageParameter.Add("total_fee", total);
            packageParameter.Add("spbill_create_ip", _webHelper.GetCurrentIpAddress());
            packageParameter.Add("notify_url", _notifyUrl);
            packageParameter.Add("trade_type", "NATIVE");


            var sign = CreateMd5Sign("key", _weiXinPaymentSettings.AppSecret, packageParameter, "utf-8");
            packageParameter.Add("sign", sign);
            var data = ParseXML(packageParameter);
            var prepayXml = HttpUtil.Send(data, OrderUrl);

            
            return prepayXml;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }


        
        
        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var customerValues = postProcessPaymentRequest.Order.DeserializeCustomValues();
            var isJsPay = false;
            if (_httpContext.Session["isJsPay"] != null)
            {
                isJsPay = _httpContext.Session["isJsPay"].ToString().ToLower() == "true";
            }

            string openId = null;
            if (isJsPay)
            {
                var weiXinAuthentication =
                    _workContext.CurrentCustomer.ExternalAuthenticationRecords.FirstOrDefault(
                        q => q.ProviderSystemName == "ExternalAuth.WeiXin");
                if (weiXinAuthentication != null)
                {
                    openId = weiXinAuthentication.ExternalIdentifier;
                }
                else
                {
                    isJsPay = false;
                }

                
            }
            

            string productId, body;
            var firstProduct = postProcessPaymentRequest.Order.OrderItems.FirstOrDefault();
            if (firstProduct != null)
            {
                productId = firstProduct.Product.Id.ToString(CultureInfo.InvariantCulture);
                body = firstProduct.Product.GetLocalized(q => q.Name);
            }
            else
            {
                productId = postProcessPaymentRequest.Order.Id.ToString(CultureInfo.InvariantCulture);
                body = postProcessPaymentRequest.Order.Id.ToString(CultureInfo.InvariantCulture);
            }

            string detail = string.Join(", ",
                postProcessPaymentRequest.Order.OrderItems.Select(q => q.Product.GetLocalized(p => p.Name)));
            string orderId = postProcessPaymentRequest.Order.Id.ToString(CultureInfo.InvariantCulture);
            string total = ((int) (postProcessPaymentRequest.Order.OrderTotal*100)).ToString(CultureInfo.InvariantCulture);

            var post = new RemotePost();
            post.FormName = "weixinpayment";
            post.Method = "POST";
            post.Add("orderid", postProcessPaymentRequest.Order.Id.ToString(CultureInfo.InvariantCulture));
            post.Add("total", postProcessPaymentRequest.Order.OrderTotal.ToString("0.00"));
            if (isJsPay && !string.IsNullOrWhiteSpace(openId))
            {
                var jsApiPay = new JsApiPay(_weiXinPaymentSettings, Path.Combine(_webHelper.GetStoreHost(_webHelper.IsCurrentConnectionSecured()), "onepagecheckout"));
                jsApiPay.Openid = openId;
                jsApiPay.TotalFee = total;

                var unifiedOrderResult = jsApiPay.GetUnifiedOrderResult(postProcessPaymentRequest.Order.Id, _webHelper.GetCurrentIpAddress(), _notifyUrl);

                var data = new WxPayData();


                var timestamp = CommonExtension.GetCurrentTimeStamp().ToString();
                var nonceStr = Guid.NewGuid().ToString("N");
                


                data.SetValue("appId", _weiXinPaymentSettings.AppId);
                data.SetValue("timeStamp", timestamp);
                data.SetValue("nonceStr", nonceStr);
                data.SetValue("package", "prepay_id=" + unifiedOrderResult.GetValue("prepay_id"));
                data.SetValue("signType", "MD5");
                var sign = data.MakeSign(_weiXinPaymentSettings.AppSecret);


                post.Add("appId", _weiXinPaymentSettings.AppId);
                post.Add("timeStamp", timestamp);
                post.Add("nonceStr", nonceStr);
                post.Add("package", "prepay_id=" + unifiedOrderResult.GetValue("prepay_id"));
                post.Add("signType", "MD5");
                post.Add("paySign", sign);

                post.Url = Path.Combine(_webHelper.GetStoreHost(_webHelper.IsCurrentConnectionSecured()), "Plugins/PaymentWeiXin/JsApiPayment/");
                post.Post();
            }
            else
            {
                post.Url = Path.Combine(_webHelper.GetStoreHost(_webHelper.IsCurrentConnectionSecured()), "Plugins/PaymentWeiXin/ProcessPayment");
                post.Add("nativeUrl", GetUrlForMethodOne(postProcessPaymentRequest.Order.Id));
                //var result = Unifiedorder(productId, body, detail, orderId, total);
                //post.Add("result", HttpUtility.HtmlEncode(result));
                post.Post();
            }
            
        }





        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _weiXinPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {

            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //WeiXin is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentWeiXin";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.WeiXin.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentWeiXin";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.WeiXin.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentWeiXinController);
        }

        public override void Install()
        {
            //settings
            var settings = new WeiXinPaymentSettings
            {
                AppId = "",
                MchId = "",
                AppSecret = "",
                AdditionalFee = 0,
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.QRCode", "二维码");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.RedirectionTip", "请用微信扫描");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.PleaseUseWechatScan", "请使用微信扫一扫");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.ScanQrcodeToPay", "扫描二维码支付");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.QrcodeOnlyValidTwoHours", "二维码有效期两小时");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.YouAreUsingWechatPay", "您正在使用微信支付");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.AppId", "AppId");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.AppId.Hint", "请输入 AppId.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.MchId", "商户ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.MchId.Hint", "请输入 MchId.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.AppSecret", "App密钥");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.AppSecret.Hint", "请输入 App密钥.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeiXin.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            
            base.Install();
        }


        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.QRCode");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.PleaseScan");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.ScanQrcodeToPay");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.QrcodeOnlyValidTwoHours");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.YouAreUsingWechatPay");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.AppId");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.AppId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.MchId");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.MchId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.AppSecret");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.AppSecret.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.WeiXin.AdditionalFee.Hint");
            
            base.Uninstall();
        }
        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return true; }
        }

        #endregion
        public string GetAccessToken()
        {
            var url =
                string.Format(
                    "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}",
                    _weiXinPaymentSettings.AppId, _weiXinPaymentSettings.AppSecret);
            var result = HttpUtil.Get(url);
            _log.Information("WeiXin access token result: " + result);
            var data = JsonMapper.ToObject(result);
            if (data["access_token"] != null && !string.IsNullOrWhiteSpace(data["access_token"].ToString()))
            {
                return data["access_token"].ToString();
            }

            throw new NopException("WeiXin access_token failed");

        }

        public void HandleEvent(OrderPaidEvent eventMessage)
        {
            if (eventMessage.Order.PaymentStatus == PaymentStatus.Paid)
            {
                _log.Information("Order Paid: " + eventMessage.Order.Id);
                
                var weixinRecord =
                    eventMessage.Order.Customer.ExternalAuthenticationRecords.FirstOrDefault(
                        q => q.ProviderSystemName == "ExternalAuth.WeiXin");

                if (weixinRecord != null)
                {
                    var openId = weixinRecord.ExternalIdentifier;

                    var postUrl = @"https://api.weixin.qq.com/cgi-bin/message/template/send?access_token=" + GetAccessToken();

                    var data = new
                    {
                        touser = openId,
                        template_id = _paidTemplateId,
                        url = Path.Combine(_webHelper.GetStoreLocation(), "orderdetails/" + eventMessage.Order.Id),
                        data = new
                        {
                            first = new
                            {
                                value = "恭喜您购买成功!",
                                color = "#173177"
                            },
                            keyword1 = new
                            {
                                value = eventMessage.Order.Id.ToString(),
                                color = "#173177"
                            },
                            keyword2 = new
                            {
                                value = eventMessage.Order.OrderTotal.ToString(_workContext.WorkingCurrency.CustomFormatting),
                                color = "#173177"
                            },
                            keyword3 = new
                            {
                                value = DateTime.UtcNow.AddHours(8).ToString("yyyy年MM月dd日 HH:mm:ss"),
                                color = "#173177"
                            },
                            remark = new
                            {
                                value = "购买成功，谢谢您的惠顾，我们将尽快安排发货!",
                                color = "#173177"
                            },
                        }
                    };
                    
                    var result = HttpUtil.Post(JsonMapper.ToJson(data), postUrl, 100, "application/json");
                    _log.Information("Weixin template message result: " + result);

                }

                
            }
        }

        

        public void HandleEvent(ShipmentSentEvent eventMessage)
        {
            _log.Information("Shipment Sent: " + eventMessage.Shipment.TrackingNumber + " Order Id: " + eventMessage.Shipment.Order.Id);

            var weixinRecord =
                eventMessage.Shipment.Order.Customer.ExternalAuthenticationRecords.FirstOrDefault(
                    q => q.ProviderSystemName == "ExternalAuth.WeiXin");

            if (weixinRecord != null)
            {
                var openId = weixinRecord.ExternalIdentifier;

                var postUrl = @"https://api.weixin.qq.com/cgi-bin/message/template/send?access_token=" + GetAccessToken();

                var data = new
                {
                    touser = openId,
                    template_id = _shippedTemplateId,
                    url = Path.Combine(_webHelper.GetStoreLocation(), "orderdetails/shipment/" + eventMessage.Shipment.Id),
                    data = new
                    {
                        first = new
                        {
                            value = "亲，宝贝已经启程向您飞来，好想快点来到你身边!",
                            color = "#173177"
                        },
                        keyword1 = new
                        {
                            value = eventMessage.Shipment.Order.Id.ToString(),
                            color = "#173177"
                        },
                        keyword2 = new
                        {
                            value = eventMessage.Shipment.AdminComment,
                            color = "#173177"
                        },
                        keyword3 = new
                        {
                            value = eventMessage.Shipment.TrackingNumber,
                            color = "#173177"
                        },
                        remark = new
                        {
                            value = "点击查看完整的物流信息 。如有问题请致电 +61 0487 538 888 或直接在微信里留言，我们将在第一时间为您服务!",
                            color = "#173177"
                        },
                    }
                };

                var result = HttpUtil.Post(JsonMapper.ToJson(data), postUrl, 100, "application/json");
                _log.Information("Weixin template message result: " + result);

            }
        }

    }
}
