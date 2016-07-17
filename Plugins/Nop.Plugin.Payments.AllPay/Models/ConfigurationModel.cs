using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.AllPay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.AllPay.MerchantId")]
        public string MerchantId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AllPay.HashKey")]
        public string HashKey { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AllPay.HashIv")]
        public string HashIv { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AllPay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AllPay.TestMode")]
        public bool TestMode { get; set; }
    }
}