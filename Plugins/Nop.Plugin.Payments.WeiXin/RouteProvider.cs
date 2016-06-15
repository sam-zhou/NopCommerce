using System.Web.Mvc;
using System.Web.Routing;

using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.WeiXin
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //Notify
            routes.MapRoute("Plugin.Payments.WeiXin.Notify",
                 "Plugins/PaymentWeiXin/Notify",
                 new { controller = "PaymentWeiXin", action = "Notify" },
                 new[] { "Nop.Plugin.Payments.WeiXin.Controllers" }
            );

            //Payment
            routes.MapRoute("Plugin.Payments.WeiXin.ProcessPayment",
                 "Plugins/PaymentWeiXin/ProcessPayment",
                 new { controller = "PaymentWeiXin", action = "ProcessPayment" },
                 new[] { "Nop.Plugin.Payments.WeiXin.Controllers" }
            );

            ////Error
            //routes.MapRoute("Plugin.Payments.WeiXin.Error",
            //     "Plugins/PaymentWeiXin/Error",
            //     new { controller = "PaymentWeiXin", action = "Error" },
            //     new[] { "Nop.Plugin.Payments.WeiXin.Controllers" }
            //);
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
