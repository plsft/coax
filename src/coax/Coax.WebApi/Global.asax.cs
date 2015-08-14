using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

using Coax.General;
using Helix.Utility;

namespace Coax.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error()
        {
            var errorContext = new ErrorContext(HttpContext.Current);
            var ex = Server.GetLastError();

            Logger.Log("Error in Api", "Context: [{0}] Error: [{1}] ", Logger.LogType.Error, errorContext, ex.ToString());
            Server.ClearError();
        }
    }
}
