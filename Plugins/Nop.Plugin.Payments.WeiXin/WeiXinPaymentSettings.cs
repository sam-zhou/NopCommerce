using Lynex.Weixin.Service;
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.WeiXin
{
    public class WeiXinPaymentSettings : ISettings, IWeiXinSettings
    {
        public string AppId { get; set; }
        public string MchId { get; set; }
        public string AppSecret { get; set; }
        public decimal AdditionalFee { get; set; }
    }
}
