﻿using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace Coax.WebApi.Controllers
{
    public class MobileController : ApiControllerBase
    {
        [Route("mobile/test")]
        [HttpGet]
        public HttpResponseMessage Test()
        {
            return Request.CreateResponse(HttpStatusCode.Accepted, new  {test =true }, new JsonMediaTypeFormatter()); 
        }
        
    }
}
