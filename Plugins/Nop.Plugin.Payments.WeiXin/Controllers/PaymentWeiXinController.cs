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
using System.IO;
using System.Web;
using Nop.Core.Domain.Logging;

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
            var error = new WeiXinPaymentErrorModel();
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.WeiXin") as WeiXinPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
            {
                error.Message = "微信支付服务终止";
            }
            else
            {
                try
                {


                    if (form.HasKeys())
                    {
                        if (!string.IsNullOrWhiteSpace(form["result"]))
                        {
                            var wxModel = new WxPayData();
                            wxModel.FromXml(HttpUtility.HtmlDecode(form["result"]), _weiXinPaymentSettings.AppSecret);

                            if (wxModel.IsSet("code_url"))
                            {
                                model.QRCode = processor.GetQrCode(wxModel.GetValue("code_url").ToString());



                                if (wxModel.IsSet("out_trade_no"))
                                {
                                    int orderId;
                                    if (int.TryParse(wxModel.GetValue("out_trade_no").ToString(), out orderId))
                                    {
                                        var order = _orderService.GetOrderById(orderId);
                                        if (order != null)
                                        {
                                            if (_orderProcessingService.CanMarkOrderAsPaid(order))
                                            {
                                                model.OrderId = order.Id.ToString();
                                                model.Total = order.OrderTotal.ToString("￥0.00");
                                            }
                                            else
                                            {
                                                if (order.PaymentStatus == PaymentStatus.Paid)
                                                {
                                                    error.Message = "您已付款，请勿重复提交";
                                                }
                                                else
                                                {
                                                    error.Message = "订单状态错误";
                                                }
                                            }

                                        }
                                        else
                                        {
                                            error.Message = "订单号不存在";
                                        }
                                    }
                                    else
                                    {
                                        error.Message = "无法读取订单号";
                                    }
                                }

                            }
                            else
                            {
                                error.Message = "无法读取二维码";
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
                catch (NopException ex)
                {
                    error.Message = ex.Message;
                }
            }

            
            
            if (error.HasError)
            {
                return View("~/Plugins/Payments.WeiXin/Views/PaymentWeiXin/Error.cshtml", error);
            }
            return View("~/Plugins/Payments.WeiXin/Views/PaymentWeiXin/ProcessPayment.cshtml", model);
        }



        [ValidateInput(false)]
        public ActionResult Notify(FormCollection form)
        {
            //接收从微信后台POST过来的数据
            var s = Request.InputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();

            _logger.InsertLog(LogLevel.Information, this.GetType() + "Receive data from WeChat : " + builder);
            //转换数据格式并验证签名
            var data = new WxPayData();
            try
            {
                data.FromXml(builder.ToString(), _weiXinPaymentSettings.AppSecret);
            }
            catch (NopException ex)
            {
                //若签名错误，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);

                Response.Write(res.ToXml());
                Response.End();
            }



            ProcessNotify(data);

            return Content("");
        }

        public void ProcessNotify(WxPayData data)
        {
            WxPayData notifyData = data;

            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
                Response.Write(res.ToXml());
                Response.End();
            }

            string transactionId = notifyData.GetValue("transaction_id").ToString();

            //查询订单，判断订单真实性
            if (!QueryOrder(transactionId))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                Response.Write(res.ToXml());
                Response.End();
            }
            //查询订单成功
            else
            {
                WxPayData res = new WxPayData();
                
                int orderId;
                if (int.TryParse(data.GetValue("out_trade_no").ToString(), out orderId))
                {
                    var order = _orderService.GetOrderById(orderId);
                    if (order != null && _orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        _orderProcessingService.MarkOrderAsPaid(order);
                        res.SetValue("return_code", "SUCCESS");
                        res.SetValue("return_msg", "OK");
                    }
                    else
                    {
                        res.SetValue("return_code", "FAIL");
                        res.SetValue("return_msg", "无法将订单设为已付");
                    }
                }

                Response.Write(res.ToXml());
                Response.End();
            }
        }

        private bool QueryOrder(string transactionId)
        {
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transactionId);
            WxPayData res = WeiXinHelper.OrderQuery(req, _weiXinPaymentSettings);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}