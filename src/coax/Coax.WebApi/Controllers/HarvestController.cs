
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using AttributeRouting.Web.Http;

namespace Coax.WebApi.Controllers
{
    public class HarvestController : ApiControllerBase
    {
        public HarvestController()
        {
        }

        [GET("test")]
        [HttpGet]
        public HttpResponseMessage Test()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { test = true }, new JsonMediaTypeFormatter());
        }

        [POST("message/save")]
        [HttpPost]
        public HttpResponseMessage SaveMessage(FormDataCollection form)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { saved = true, formdata = form }, new JsonMediaTypeFormatter());
        }



    }
}
