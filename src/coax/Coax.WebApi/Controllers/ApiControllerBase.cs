
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Coax.WebApi.Controllers
{
    public class ApiControllerBase : ApiController
    {
        public Stopwatch Stopwatch { get; set;  } 
        public ApiControllerBase()
        {
            Stopwatch = Stopwatch.StartNew();
        }

        [HttpOptions]
        [HttpGet]
        public HttpResponseMessage Options()
        {
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        }
    }
}