using Nop.Web.Framework.Mvc.Routes;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nop.Plugin.ExternalAuth.Google
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.ExternalAuth.Google.Login",
                 "Plugins/ExternalAuthGoogle/Login",
                 new { controller = "ExternalAuthGoogle", action = "Login" },
                 new[] { "Nop.Plugin.ExternalAuth.Google.Controllers" }
            );

            routes.MapRoute("Plugin.ExternalAuth.Google.LoginCallback",
                 "Plugins/ExternalAuthGoogle/LoginCallback",
                 new { controller = "ExternalAuthGoogle", action = "LoginCallback" },
                 new[] { "Nop.Plugin.ExternalAuth.Google.Controllers" }
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
