﻿using System.Linq;
using System.Web.Http;

namespace Coax.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");

            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
            config.Filters.Add( new CrossSiteScriptingAttribute());

            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "CoaxAPI",
                routeTemplate: "{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional, action = "Get" });


        }
    }
}
