
namespace Helix.Utility
{
    using System.Web;

    public sealed class UserContext
    {

        public UserContext(string user = "")
        {
            UsernameDefaulted = user;
        }

        public static string User
        {
            get
            {
                return HttpContext.Current != null ? HttpContext.Current.User.Identity.Name : "Internal";
            }
        }

        public string UsernameDefaulted { get; set; }
    }
}
