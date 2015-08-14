using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using AttributeRouting.Web.Http;

namespace Coax.WebApi.Controllers
{
    public class MobileController : ApiControllerBase
    {
        [GET("/mobile/test")]
        [HttpGet]
        public HttpResponseMessage Test()
        {
            return Request.CreateResponse(HttpStatusCode.Accepted, new  {test =true }, new JsonMediaTypeFormatter()); 
        }
        
    }
}
