using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.AllPay
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //Notify
            routes.MapRoute("Plugin.Payments.AllPay.Notify",
                 "Plugins/PaymentAllPay/Notify",
                 new { controller = "PaymentAllPay", action = "Notify" },
                 new[] { "Nop.Plugin.Payments.AllPay.Controllers" }
            );

            //Payment
            routes.MapRoute("Plugin.Payments.AllPay.ProcessPayment",
                 "Plugins/PaymentAllPay/ProcessPayment",
                 new { controller = "PaymentAllPay", action = "ProcessPayment" },
                 new[] { "Nop.Plugin.Payments.AllPay.Controllers" }
            );

            //Notify
            routes.MapRoute("Plugin.Payments.AllPay.Return",
                 "Plugins/PaymentAllPay/Return",
                 new { controller = "PaymentAllPay", action = "Return" },
                 new[] { "Nop.Plugin.Payments.AllPay.Controllers" }
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
