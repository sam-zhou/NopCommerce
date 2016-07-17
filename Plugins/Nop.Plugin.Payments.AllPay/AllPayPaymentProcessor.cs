using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Routing;
using AllPay.Payment.Integration;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.AllPay.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using static System.String;

namespace Nop.Plugin.Payments.AllPay
{
    /// <summary>
    /// AllPay payment processor
    /// </summary>
    public class AllPayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly AllPayPaymentSettings _allPayPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private const string CheckoutUrl = @"https://payment.allpay.com.tw/Cashier/AioCheckOut/V2";
        private const string TestCheckoutUrl = @"https://payment-stage.allpay.com.tw/Cashier/AioCheckOut/V2";
        #endregion



        #region Ctor

        public AllPayPaymentProcessor(AllPayPaymentSettings allPayPaymentSettings,
            ISettingService settingService, IWebHelper webHelper,
            IStoreContext storeContext)
        {
            this._allPayPaymentSettings = allPayPaymentSettings;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets MD5 hash
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="inputCharset">Input charset</param>
        /// <returns>Result</returns>
        public string GetMd5(string input, string inputCharset)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding(inputCharset).GetBytes(input));
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString();
        }




        /// <summary>
        /// Gets HTTP
        /// </summary>
        /// <param name="StrUrl">Url</param>
        /// <param name="Timeout">Timeout</param>
        /// <returns>Result</returns>
        public string Get_Http(string StrUrl, int Timeout)
        {
            string strResult = Empty;
            try
            {
                HttpWebRequest myReq = (HttpWebRequest)HttpWebRequest.Create(StrUrl);
                myReq.Timeout = Timeout;
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

        /// <summary>
        /// Bubble sort
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>Result</returns>
        public string[] BubbleSort(string[] input)
        {
            for (var i = 0; i < input.Length; i++)
            {
                var exchange = false;

                int j;
                for (j = input.Length - 2; j >= i; j--)
                {
                    if (CompareOrdinal(input[j + 1], input[j]) < 0)
                    {
                        var temp = input[j + 1];
                        input[j + 1] = input[j];
                        input[j] = temp;

                        exchange = true;
                    }
                }

                if (!exchange)
                {
                    break;
                }
            }
            return input;
        }

        public string GetCheckValueFromStringList(string[] param)
        {
            var sortedParam = BubbleSort(param);
            var paramString = "HashKey=" + _allPayPaymentSettings.HashKey;
            foreach (var s in sortedParam)
            {
                paramString += "&" + s;
            }
            paramString += "&HashIV=" + _allPayPaymentSettings.HashIv;
            paramString = paramString.ToLower();
            paramString = HttpUtility.UrlEncode(paramString);
            return GetMd5(paramString, "utf-8").ToUpper();
        }

        public string GetCheckValue(NameValueCollection param)
        {
            var paramList = new List<string>();
            foreach (var key in param.AllKeys)
            {
                paramList.Add(key + "=" + param[key]);
            }
            return GetCheckValueFromStringList(paramList.ToArray());            
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
            var langId = _storeContext.CurrentStore.DefaultLanguageId;
            var itemName = Join(",", postProcessPaymentRequest.Order.OrderItems.Select(q => q.Product.GetLocalized(x => x.Name, langId)));
            if (itemName.Length > 200)
            {
                itemName = itemName.Substring(0, 200);
            }



            var post = new RemotePost {FormName = "AllPaysubmit"};

            post.Url = _allPayPaymentSettings.TestMode ? TestCheckoutUrl : CheckoutUrl;
            post.Add("MerchantID", _allPayPaymentSettings.MerchantId);
            post.Add("MerchantTradeNo", postProcessPaymentRequest.Order.Id.ToString());
            post.Add("MerchantTradeDate", DateTime.UtcNow.AddHours(8).ToString("yyyy/MM/dd HH:mm:ss"));
            post.Add("PaymentType", "aio");
            post.Add("TotalAmount", postProcessPaymentRequest.Order.OrderTotal.ToString("N0"));
            post.Add("TradeDesc", "寧尼可進口線上商城");
            post.Add("ItemName", itemName);
            post.Add("ReturnURL", ReturnUrl);
            post.Add("ChoosePayment", "ALL");
            post.Add("ClientBackURL", Path.Combine(_webHelper.GetStoreLocation(), "orderdetails/" + postProcessPaymentRequest.Order.Id));
            var checkMacValue = GetCheckValue(post.Params);
            post.Add("CheckMacValue", checkMacValue);

            //post.Url = Path.Combine(_webHelper.GetStoreHost(_webHelper.IsCurrentConnectionSecured()), "Plugins/PaymentAllPay/ProcessPayment");
            //post.Add("url", url);
            //post.Add("orderid", postProcessPaymentRequest.Order.Id.ToString(CultureInfo.InvariantCulture));
            //post.Add("total", postProcessPaymentRequest.Order.OrderTotal.ToString("0.00"));
            post.Post();
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
            return _allPayPaymentSettings.AdditionalFee;
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

            //AllPay is the redirection payment method
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
            controllerName = "PaymentAllPay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.AllPay.Controllers" }, { "area", null } };
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
            controllerName = "PaymentAllPay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.AllPay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentAllPayController);
        }

        public override void Install()
        {
            //settings
            var settings = new AllPayPaymentSettings()
            {
                MerchantId = "",
                HashKey = "",
                HashIv= "",
                AdditionalFee = 0,
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AllPay.RedirectionTip", "您將被轉至歐付寶完成付款");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AllPay.TestMode", "Test Mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AllPay.MerchantId", "Merchant Id");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AllPay.MerchantId.Hint", "Enter Merchant Id.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AllPay.HashKey", "HashKey");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AllPay.HashKey.Hint", "Enter HashKey.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AllPay.HashIv", "HashIv");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AllPay.HashIv.Hint", "Enter HashIv.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AllPay.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AllPay.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AllPay.YouAreUsingAllPay", "您正在使用歐付寶");
            base.Install();
        }


        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.AllPay.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.AllPay.TestMode");
            this.DeletePluginLocaleResource("Plugins.Payments.AllPay.MerchantId");
            this.DeletePluginLocaleResource("Plugins.Payments.AllPay.MerchantId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AllPay.HashKey");
            this.DeletePluginLocaleResource("Plugins.Payments.AllPay.HashKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AllPay.HashIv");
            this.DeletePluginLocaleResource("Plugins.Payments.AllPay.HashIv.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AllPay.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.AllPay.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AllPay.YouAreUsingAllPay");
            base.Uninstall();
        }
        #endregion

        #region Properies

        public string ReturnUrl
        {
            get
            {
                return Path.Combine(_webHelper.GetStoreLocation(), "Plugins/PaymentAllPay/Notify");
            }
        }

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
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
    }
}
