using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.WeiXin.Models;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.WeiXin.Controllers
{
    public class PaymentWeiXinController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly WeiXinPaymentSettings _weiXinPaymentSettings;
        private readonly PaymentSettings _paymentSettings;

        public PaymentWeiXinController(ISettingService settingService, 
            IPaymentService paymentService, IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            ILogger logger, IWebHelper webHelper, IWorkContext workContext,
            WeiXinPaymentSettings weiXinPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._webHelper = webHelper;
            _workContext = workContext;
            this._weiXinPaymentSettings = weiXinPaymentSettings;
            this._paymentSettings = paymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.AppId = _weiXinPaymentSettings.AppId;
            model.MchId = _weiXinPaymentSettings.MchId;
            model.AppSecret = _weiXinPaymentSettings.AppSecret;
            model.AdditionalFee = _weiXinPaymentSettings.AdditionalFee;

            return View("~/Plugins/Payments.WeiXin/Views/PaymentWeiXin/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _weiXinPaymentSettings.AppId = model.AppId;
            _weiXinPaymentSettings.MchId = model.MchId;
            _weiXinPaymentSettings.AppSecret = model.AppSecret;
            _weiXinPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_weiXinPaymentSettings);
            
            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            return View("~/Plugins/Payments.WeiXin/Views/PaymentWeiXin/PaymentInfo.cshtml", model);
        }


        

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [HttpPost]
        public ActionResult ProcessPayment(FormCollection form)
        {
            var model = new WeiXinPaymentModel();
            if (form.HasKeys() && !string.IsNullOrWhiteSpace(form["QRCode"]))
            {
                model.QRCode = form["QRCode"];
            }
            return View("~/Plugins/Payments.WeiXin/Views/PaymentWeiXin/ProcessPayment.cshtml", model);
        }

        //[NonAction]
        //public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        //{
        //    var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.WeiXin") as WeiXinPaymentProcessor;
        //    if (processor == null ||
        //        !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
        //        throw new NopException("WeiXin module cannot be loaded");


        //    var paymentInfo = new ProcessPaymentRequest();
        //    paymentInfo.OrderGuid = Guid.NewGuid();
        //    paymentInfo.CustomerId = _workContext.CurrentCustomer.Id;
        //    paymentInfo.CustomValues = processor.Unifiedorder();
        //    return paymentInfo;
        //}

        [ValidateInput(false)]
        public ActionResult Notify(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.WeiXin") as WeiXinPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("WeiXin module cannot be loaded");


            string weixinNotifyUrl = "https://www.WeiXin.com/cooperate/gateway.do?service=notify_verify";
            string openId = _weiXinPaymentSettings.AppSecret;
            if (string.IsNullOrEmpty(openId))
                throw new Exception("OpenId is not set");
            string mchId = _weiXinPaymentSettings.MchId;
            if (string.IsNullOrEmpty(mchId))
                throw new Exception("MchId is not set");
            string _input_charset = "utf-8";

            weixinNotifyUrl = weixinNotifyUrl + "&partner=" + openId + "&notify_id=" + Request.Form["notify_id"];
            string responseTxt = processor.Get_Http(weixinNotifyUrl, 120000);

            int i;
            NameValueCollection coll;
            coll = Request.Form;
            String[] requestarr = coll.AllKeys;
            string[] Sortedstr = requestarr;

            var prestr = new StringBuilder();
            for (i = 0; i < Sortedstr.Length; i++)
            {
                if (Request.Form[Sortedstr[i]] != "" && Sortedstr[i] != "sign" && Sortedstr[i] != "sign_type")
                {
                    if (i == Sortedstr.Length - 1)
                    {
                        prestr.Append(Sortedstr[i] + "=" + Request.Form[Sortedstr[i]]);
                    }
                    else
                    {
                        prestr.Append(Sortedstr[i] + "=" + Request.Form[Sortedstr[i]] + "&");

                    }
                }
            }

            prestr.Append(mchId);

            string mysign = processor.GetMD5(prestr.ToString(), _input_charset);

            string sign = Request.Form["sign"];

            if (mysign == sign && responseTxt == "true")
            {
                if (Request.Form["trade_status"] == "WAIT_BUYER_PAY")
                {
                    string strOrderNo = Request.Form["out_trade_no"];
                    string strPrice = Request.Form["total_fee"];
                }
                else if (Request.Form["trade_status"] == "TRADE_FINISHED" || Request.Form["trade_status"] == "TRADE_SUCCESS")
                {
                    string strOrderNo = Request.Form["out_trade_no"];
                    string strPrice = Request.Form["total_fee"];

                    int orderId = 0;
                    if (Int32.TryParse(strOrderNo, out orderId))
                    {
                        var order = _orderService.GetOrderById(orderId);
                        if (order != null && _orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            _orderProcessingService.MarkOrderAsPaid(order);
                        }
                    }
                }
                else
                {
                }

                Response.Write("success");
            }
            else
            {
                Response.Write("fail");
                string logStr = "MD5:mysign=" + mysign + ",sign=" + sign + ",responseTxt=" + responseTxt;
                _logger.Error(logStr);
            }

            return Content("");
        }
    }
}