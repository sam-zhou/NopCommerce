namespace Nop.Plugin.Payments.AllPay.Models
{
    public class AllPayPaymentErrorModel
    {
        public string Message { get; set; }

        public string Title { get; set; }

        public bool HasError
        {
            get { return !string.IsNullOrWhiteSpace(Message); }
        }
    }
}
