using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Coax.General
{
    public sealed class Crypto
    {
        private static object o;

        static Crypto()
        {
            o = new object();
        }

        public static string HashString(string value)
        {
            lock (o)
            {
                const string salt = ":!@#$%^&";
                var sha1 = new SHA1CryptoServiceProvider();
                var result = sha1.ComputeHash(Encoding.UTF8.GetBytes(value + salt));
                var hex = result.Select(h => h.ToString("x2"));
                return string.Join("", hex);
            }
        }
    }
}