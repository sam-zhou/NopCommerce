using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.AllPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.AllPay.Controllers
{
    public class PaymentAllPayController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly AllPayPaymentSettings _allPayPaymentSettings;
        private readonly PaymentSettings _paymentSettings;

        public PaymentAllPayController(ISettingService settingService, 
            IPaymentService paymentService, IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            ILogger logger, IWebHelper webHelper,
            AllPayPaymentSettings allPayPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._webHelper = webHelper;
            this._allPayPaymentSettings = allPayPaymentSettings;
            this._paymentSettings = paymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.TestMode = _allPayPaymentSettings.TestMode;
            model.MerchantId = _allPayPaymentSettings.MerchantId;
            model.HashKey = _allPayPaymentSettings.HashKey;
            model.HashIv = _allPayPaymentSettings.HashIv;
            model.AdditionalFee = _allPayPaymentSettings.AdditionalFee;

            return View("~/Plugins/Payments.AllPay/Views/PaymentAllPay/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _allPayPaymentSettings.TestMode = model.TestMode;
            _allPayPaymentSettings.MerchantId = model.MerchantId;
            _allPayPaymentSettings.HashKey = model.HashKey;
            _allPayPaymentSettings.HashIv = model.HashIv;
            _allPayPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_allPayPaymentSettings);
            
            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            return View("~/Plugins/Payments.AllPay/Views/PaymentAllPay/PaymentInfo.cshtml", model);
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
            var error = new AllPayPaymentErrorModel();
            var model = new AllPayPaymentModel();
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.AllPay") as AllPayPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
            {
                error.Message = "支付宝服务终止";
            }
            else
            {
                if (form.HasKeys())
                {
                    if (!string.IsNullOrWhiteSpace(form["url"]))
                    {
                        model.Url = form["url"];

                        if (!string.IsNullOrWhiteSpace(form["orderid"]))
                        {
                            model.OrderId = form["orderid"];


                            if (!string.IsNullOrWhiteSpace(form["total"]))
                            {
                                model.Total = form["total"];
                            }
                            else
                            {
                                error.Message = "参数错误Total";
                            }

                        }
                        else
                        {
                            error.Message = "参数错误OrderId";
                        }

                    }
                    else
                    {
                        error.Message = "参数错误";
                    }
                }
                else
                {
                    error.Message = "没有参数";
                }
            }



            if (error.HasError)
            {
                return View("~/Plugins/Payments.AllPay/Views/PaymentAllPay/Error.cshtml", error);
            }
            return View("~/Plugins/Payments.AllPay/Views/PaymentAllPay/ProcessPayment.cshtml", model);
        }


        [ValidateInput(false)]
        public ActionResult Notify(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.AllPay") as AllPayPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("AllPay module cannot be loaded");
            int orderId;
            var errorResponse = string.Empty;

            if (form["MerchantID"] != _allPayPaymentSettings.MerchantId)
            {
                errorResponse = "0|MerchantID not match";
            }
            else if (string.IsNullOrWhiteSpace(form["MerchantTradeNo"]))
            {
                errorResponse = "0|MerchantTradeNo missing";
            }
            else if (!int.TryParse(form["MerchantTradeNo"], out orderId))
            {
                errorResponse = "0|MerchantTradeNo invalid";
            }
            else
            {
                var param = new List<string>();
                foreach (var key in form.AllKeys)
                {
                    if (key != "CheckMacValue")
                    {
                        param.Add(key + "=" + form[key]);
                    }
                }
                var mysign = processor.GetCheckValueFromStringList(param.ToArray());
                if (mysign == form["CheckMacValue"])
                {
                    var order = _orderService.GetOrderById(orderId);
                    if (order != null)
                    {
                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            if (form["RtnCode"] == "1")
                            {
                                _orderProcessingService.MarkOrderAsPaid(order);
                            }
                            else
                            {
                                errorResponse = "0|Unpaid order";
                            }
                        }
                        else
                        {
                            errorResponse = "0|Cannot mark order as paid";
                        }
                    }
                    else
                    {
                        errorResponse = "0|MerchantTradeNo invalid";
                    }
                }
                else
                {
                    errorResponse = "0|Sign invalid";
                }
                


            }


            if (errorResponse != string.Empty)
            {
                var formString = string.Empty;

                foreach (var key in form.AllKeys)
                {
                    if (formString != string.Empty)
                    {
                        formString += "&";
                    }
                    formString += key + "=" + form[key];
                }

                string logStr = "AllPay Failed: " + errorResponse + " " + formString;
                _logger.Error(logStr);
                Response.Write(errorResponse);
            }
            else
            {
                Response.Write("1|OK");
            }

            return Content("");
        }

        [ValidateInput(false)]
        public ActionResult Return()
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.AllPay") as AllPayPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("AllPay module cannot be loaded");

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}