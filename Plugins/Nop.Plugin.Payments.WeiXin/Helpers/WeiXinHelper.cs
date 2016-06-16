using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Payments.WeiXin.Models;

namespace Nop.Plugin.Payments.WeiXin
{
    public static class WeiXinHelper
    {
        public static WxPayData OrderQuery(WxPayData inputObj, WeiXinPaymentSettings settings, int timeOut = 6)
        {
            string url = "https://api.mch.weixin.qq.com/pay/orderquery";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no") && !inputObj.IsSet("transaction_id"))
            {
                throw new NopException("订单查询接口中，out_trade_no、transaction_id至少填一个！");
            }

            inputObj.SetValue("appid", settings.AppId);//公众账号ID
            inputObj.SetValue("mch_id", settings.MchId);//商户号
            inputObj.SetValue("nonce_str", Guid.NewGuid().ToString("N"));//随机字符串
            inputObj.SetValue("sign", inputObj.MakeSign(settings.AppSecret));//签名

            string xml = inputObj.ToXml();

            var start = DateTime.Now;

            string response = HttpUtil.Post(xml, url, timeOut);//调用HTTP通信接口提交数据


            var end = DateTime.Now;
            int timeCost = (int)((end - start).TotalMilliseconds);//获得接口耗时

            //将xml格式的数据转化为对象以返回
            WxPayData result = new WxPayData();
            result.FromXml(response, settings.AppSecret);

            ReportCostTime(url, timeCost, result, settings);//测速上报

            return result;
        }

        public static int ReportLevenl
        {
            get { return 1; }
        }

        /**
* 
* 统一下单
* @param WxPaydata inputObj 提交给统一下单API的参数
* @param int timeOut 超时时间
* @throws NopException
* @return 成功时返回，其他抛异常
*/
        public static WxPayData UnifiedOrder(WxPayData inputObj, string clientIp, string notifyUrl, WeiXinPaymentSettings settings, int timeOut = 6)
        {
            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no"))
            {
                throw new NopException("缺少统一支付接口必填参数out_trade_no！");
            }
            else if (!inputObj.IsSet("body"))
            {
                throw new NopException("缺少统一支付接口必填参数body！");
            }
            else if (!inputObj.IsSet("total_fee"))
            {
                throw new NopException("缺少统一支付接口必填参数total_fee！");
            }
            else if (!inputObj.IsSet("trade_type"))
            {
                throw new NopException("缺少统一支付接口必填参数trade_type！");
            }

            //关联参数
            if (inputObj.GetValue("trade_type").ToString() == "JSAPI" && !inputObj.IsSet("openid"))
            {
                throw new NopException("统一支付接口中，缺少必填参数openid！trade_type为JSAPI时，openid为必填参数！");
            }
            if (inputObj.GetValue("trade_type").ToString() == "NATIVE" && !inputObj.IsSet("product_id"))
            {
                throw new NopException("统一支付接口中，缺少必填参数product_id！trade_type为JSAPI时，product_id为必填参数！");
            }

            //异步通知url未设置，则使用配置文件中的url
            if (!inputObj.IsSet("notify_url"))
            {
                inputObj.SetValue("notify_url", notifyUrl);//异步通知url
            }

            inputObj.SetValue("appid", settings.AppId);//公众账号ID
            inputObj.SetValue("mch_id", settings.MchId);//商户号
            inputObj.SetValue("spbill_create_ip", clientIp);//终端ip	  	    
            inputObj.SetValue("nonce_str", Guid.NewGuid().ToString("N"));//随机字符串

            //签名
            inputObj.SetValue("sign", inputObj.MakeSign(settings.AppSecret));
            string xml = inputObj.ToXml();

            var start = DateTime.Now;

            string response = HttpUtil.Post(xml, url, timeOut);


            var end = DateTime.Now;
            var timeCost = (int)((end - start).TotalMilliseconds);

            var result = new WxPayData();
            result.FromXml(response, settings.AppSecret);

            ReportCostTime(url, timeCost, result, settings);//测速上报

            return result;
        }

        public static string GenerateTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        private static void ReportCostTime(string interfaceUrl, int timeCost, WxPayData inputObj, WeiXinPaymentSettings settings)
        {
            //如果不需要进行上报
            if (ReportLevenl == 0)
            {
                return;
            }

            //如果仅失败上报
            if (ReportLevenl == 1 && inputObj.IsSet("return_code") && inputObj.GetValue("return_code").ToString() == "SUCCESS" &&
             inputObj.IsSet("result_code") && inputObj.GetValue("result_code").ToString() == "SUCCESS")
            {
                return;
            }

            //上报逻辑
            WxPayData data = new WxPayData();
            data.SetValue("interface_url", interfaceUrl);
            data.SetValue("execute_time_", timeCost);
            //返回状态码
            if (inputObj.IsSet("return_code"))
            {
                data.SetValue("return_code", inputObj.GetValue("return_code"));
            }
            //返回信息
            if (inputObj.IsSet("return_msg"))
            {
                data.SetValue("return_msg", inputObj.GetValue("return_msg"));
            }
            //业务结果
            if (inputObj.IsSet("result_code"))
            {
                data.SetValue("result_code", inputObj.GetValue("result_code"));
            }
            //错误代码
            if (inputObj.IsSet("err_code"))
            {
                data.SetValue("err_code", inputObj.GetValue("err_code"));
            }
            //错误代码描述
            if (inputObj.IsSet("err_code_des"))
            {
                data.SetValue("err_code_des", inputObj.GetValue("err_code_des"));
            }
            //商户订单号
            if (inputObj.IsSet("out_trade_no"))
            {
                data.SetValue("out_trade_no", inputObj.GetValue("out_trade_no"));
            }
            //设备号
            if (inputObj.IsSet("device_info"))
            {
                data.SetValue("device_info", inputObj.GetValue("device_info"));
            }

            try
            {
                Report(data, settings);
            }
            catch (NopException ex)
            {
                //不做任何处理
            }
        }

        public static WxPayData Report(WxPayData inputObj, WeiXinPaymentSettings settings, int timeOut = 1)
        {
            string url = "https://api.mch.weixin.qq.com/payitil/report";
            //检测必填参数
            if (!inputObj.IsSet("interface_url"))
            {
                throw new NopException("接口URL，缺少必填参数interface_url！");
            }
            if (!inputObj.IsSet("return_code"))
            {
                throw new NopException("返回状态码，缺少必填参数return_code！");
            }
            if (!inputObj.IsSet("result_code"))
            {
                throw new NopException("业务结果，缺少必填参数result_code！");
            }
            if (!inputObj.IsSet("user_ip"))
            {
                throw new NopException("访问接口IP，缺少必填参数user_ip！");
            }
            if (!inputObj.IsSet("execute_time_"))
            {
                throw new NopException("接口耗时，缺少必填参数execute_time_！");
            }

            inputObj.SetValue("appid", settings.AppId);//公众账号ID
            inputObj.SetValue("mch_id", settings.MchId);//商户号
            inputObj.SetValue("user_ip", "23.97.79.119");//终端ip
            inputObj.SetValue("time", DateTime.Now.ToString("yyyyMMddHHmmss"));//商户上报时间	 
            inputObj.SetValue("nonce_str", Guid.NewGuid().ToString("N"));//随机字符串
            inputObj.SetValue("sign", inputObj.MakeSign(settings.AppSecret));//签名
            string xml = inputObj.ToXml();

            
            string response = HttpUtil.Post(xml, url, timeOut);

            
            WxPayData result = new WxPayData();
            result.FromXml(response, settings.AppSecret);
            return result;
        }


    }
}
