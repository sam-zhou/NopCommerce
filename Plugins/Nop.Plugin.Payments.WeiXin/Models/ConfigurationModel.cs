using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.WeiXin.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.WeiXin.AppId")]
        public string AppId { get; set; }
        [NopResourceDisplayName("Plugins.Payments.WeiXin.MchId")]
        public string MchId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WeiXin.OpenId")]
        public string OpenId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WeiXin.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
    }
}