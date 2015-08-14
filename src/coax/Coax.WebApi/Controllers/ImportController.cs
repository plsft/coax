using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using AttributeRouting.Web.Http;

namespace Coax.WebApi.Controllers
{
    public class ImportController : ApiControllerBase
    {
        [GET("/import/test")]
        [HttpGet]
        public HttpResponseMessage Test()
        {
            return Request.CreateResponse(HttpStatusCode.Accepted, new { test = true }, new JsonMediaTypeFormatter());
        }
    }
}
