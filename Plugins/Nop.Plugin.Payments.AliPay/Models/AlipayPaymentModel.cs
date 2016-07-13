using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AliPay.Models
{
    public class AlipayPaymentModel
    {
        public string Url { get; set; }

        public string OrderId { get; set; }

        public string Total { get; set; }
    }
}
