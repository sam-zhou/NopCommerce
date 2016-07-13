namespace Nop.Plugin.Payments.AliPay.Models
{
    public class AlipayPaymentErrorModel
    {
        public string Message { get; set; }

        public string Title { get; set; }

        public bool HasError
        {
            get { return !string.IsNullOrWhiteSpace(Message); }
        }
    }
}
