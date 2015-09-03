using System;
using System.Web.Security;

namespace Helix.Security
{
    public sealed class Auth
    {
        public static string GetAuthTicket(int version, string userName, string userData, int expiresInMin = 30, bool persistent = true)
        {
            var ticket = new FormsAuthenticationTicket(version, userName, DateTime.Now, DateTime.Now.AddMinutes(expiresInMin), persistent, userData);
            return FormsAuthentication.Encrypt(ticket);
        }
    }
}
