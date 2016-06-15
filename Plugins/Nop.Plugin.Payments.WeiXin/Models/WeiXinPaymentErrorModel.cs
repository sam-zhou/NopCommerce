using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.WeiXin.Models
{
    public class WeiXinPaymentErrorModel
    {
        public string Message { get; set; }

        public string Title { get; set; }

        public bool HasError
        {
            get { return !string.IsNullOrWhiteSpace(Message); }
        }
    }
}
