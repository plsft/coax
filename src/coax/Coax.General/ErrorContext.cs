 
using System.Web;

namespace Coax.General
{
    public sealed class ErrorContext
    {
        private readonly HttpContext _ctx;

        public ErrorContext(HttpContext ctx)
        {
            if (ctx == null)
                _ctx = new HttpContext(new HttpRequest("", "", ""), new HttpResponse(null));

            _ctx = ctx;
        }

        public string ApplicationFilePath
        {
            get
            {
                return _ctx.Request.PhysicalApplicationPath;
            }
        }

        public string RawUrl
        {
            get
            {
                return _ctx.Request.RawUrl;
            }
        }

        public string Method
        {
            get
            {
                return _ctx.Request.HttpMethod;
            }
        }

        public string UserAgent
        {
            get
            {
                return _ctx.Request.UserAgent;
            }
        }

        public string UserAddress
        {
            get
            {
                return _ctx.Request.UserHostAddress;
            }
        }

        public string Username
        {
            get
            {
                return _ctx.User.Identity.Name;
            }
        }

        public override string ToString()
        {
            return string.Format("ErrorContext=FilePath=[{0}], RawUrl=[{1}, Method=[{2}, UA={3}, HostAddress=[{4}], User=[{5}]",
                ApplicationFilePath, RawUrl, Method, UserAgent, UserAddress, Username);
        }
    }
}
