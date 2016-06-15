using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.WeiXin.Models
{
    public class WeiXinPaymentModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.WeiXin.QRCode")]
        [AllowHtml]
        public string QRCode { get; set; }



    }
}