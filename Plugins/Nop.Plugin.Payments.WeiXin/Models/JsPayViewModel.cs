using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.WeiXin.Models
{
    public class JsPayViewModel
    {
        public string AppId { get; set; }
        public string TimeStamp { get; set; }
        public string NonceStr { get; set; }
        public string Package { get; set; }
        public string SignType { get; set; }
        public string PaySign { get; set; }
        public string OrderId { get; set; }
        public string Total { get; set; }
    }
}
