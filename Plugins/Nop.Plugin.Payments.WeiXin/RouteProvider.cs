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

            //Return
            routes.MapRoute("Plugin.Payments.WeiXin.Return",
                 "Plugins/PaymentWeiXin/Return",
                 new { controller = "PaymentWeiXin", action = "Return" },
                 new[] { "Nop.Plugin.Payments.WeiXin.Controllers" }
            );

            //GetOrder
            routes.MapRoute("Plugin.Payments.WeiXin.GetOrder",
                 "Plugins/PaymentWeiXin/GetOrder",
                 new { controller = "PaymentWeiXin", action = "GetOrder" },
                 new[] { "Nop.Plugin.Payments.WeiXin.Controllers" }
            );
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
