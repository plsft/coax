
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
        public HttpResponseMessage Options()
        {
            return null; 
        }
    }
}