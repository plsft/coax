using System.Web.Http.Filters;

namespace Coax.WebApi
{
    public class CrossSiteScriptingAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response != null)
            {
                actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, DELETE");
                actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Headers", "content-type, accept");
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}