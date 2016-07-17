using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.AllPay
{
    public class AllPayPaymentSettings : ISettings
    {
        public string MerchantId { get; set; }
        public string HashKey { get; set; }
        public string HashIv { get; set; }
        public decimal AdditionalFee { get; set; }

        public bool TestMode { get; set; }
    }
}
