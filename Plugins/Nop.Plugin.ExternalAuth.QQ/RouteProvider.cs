using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.ExternalAuth.QQ
{
    public class RouteProvider : IRouteProvider
    {

        public void RegisterRoutes(RouteCollection routes)
        {
            // Login
            routes.MapRoute("Plugin.ExternalAuth.QQ.Login",
                "Plugins/ExternalAuthQQ/Login",
                new { controller = "ExternalAuthQQ", action = "Login" },
                new[] { "Nop.Plugin.ExternalAuth.QQ.Controllers" });

            // LoginCallback
            routes.MapRoute("Plugin.ExternalAuth.QQ.LoginCallback",
                "Plugins/ExternalAuthQQ/LoginCallback",
                new { controller = "ExternalAuthQQ", action = "LoginCallback" },
                new[] { "Nop.Plugin.ExternalAuth.QQ.Controllers" });
        }

        public int Priority
        {
            get { return 0; }
        }
    }
}
