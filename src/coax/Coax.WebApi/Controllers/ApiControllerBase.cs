
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Coax.WebApi.Controllers
{
    public class ApiControllerBase : ApiController
    {
        public ApiControllerBase()
        {
        }

        [HttpOptions]
        [HttpGet]
        public HttpResponseMessage Options()
        {
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        }
    }
}