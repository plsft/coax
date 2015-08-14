
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using AttributeRouting.Web.Http;

namespace Coax.WebApi.Controllers
{
    public class HarvestController : ApiControllerBase
    {
        /*
            POST http://localhost:55706/harvest/savemessage HTTP/1.1
            Host: localhost:55706
            Content-Length: 6
            Content-Type: application/x-www-form-urlencoded
        */

        [GET("test")]
        [HttpGet]
        public HttpResponseMessage Test()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { test = true }, new JsonMediaTypeFormatter());
        }

        [POST("savemessage")]
        [HttpPost]
        public HttpResponseMessage SaveMessage(FormDataCollection form)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { saved = true, formdata = form }, new JsonMediaTypeFormatter());
        }



    }
}
