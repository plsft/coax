using System;
using System.Web.Security;

namespace Helix.Security
{
    public sealed class Auth
    {
        public static string GetAuthTicket(int version, string userName, string userData, string domain, int expiresInMin = 30, bool persistent = true)
        {
            var ticket = new FormsAuthenticationTicket(version, userName, DateTime.Now, DateTime.Now.AddMinutes(expiresInMin), persistent, userData, domain);
            return FormsAuthentication.Encrypt(ticket);
        }
    }
}
