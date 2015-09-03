using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace Helix.Security
{
    using System.Web.Security;

    public sealed class Crypto
    {
        public enum EncFormat
        {
            MD5 = 0,
            SHA1 = 1,
            Default = (MD5)
        }

        public static string PassPhrase = "defaultValue1234567890";
        public static string SaltValue = "defaultValue1234567890";
        public static int PasswordInterations = 4;
        public static string InitVector = "@1B2c3D4e5F6g7H8";
        public static int KeySize = 256;
        public static EncFormat EncryptFormat = EncFormat.MD5;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ef"></param>
        /// <returns></returns>
        public static string SimpleHash(string text, EncFormat ef)
        {
            var enc = string.Empty;
            var encString = string.Empty;

            if (text.Length > 0)
            {
                enc = GetEncFormat();
                encString = FormsAuthentication.HashPasswordForStoringInConfigFile(text, enc);
            }

            return encString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string HashString(string value)
        {
            const string salt = ":!@#$%^&";
            var sha1 = new SHA1CryptoServiceProvider();
            var result = sha1.ComputeHash(Encoding.UTF8.GetBytes(value + salt));
            var hex = result.Select(h => h.ToString("x2"));
            return string.Join("", hex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string Encrypt(string val)
        {
            var hash = GetEncFormat();
            return CoreEncryptionBase.Encrypt(val, PassPhrase, SaltValue, hash, PasswordInterations, InitVector, KeySize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string Decrypt(string val)
        {
            var hash = GetEncFormat();
            return CoreEncryptionBase.Decrypt(val, PassPhrase, SaltValue, hash, PasswordInterations, InitVector, KeySize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetEncFormat()
        {
            var hashType = "MD5";

            try
            {
                switch (EncryptFormat)
                {
                    case EncFormat.MD5: hashType = "MD5"; break;
                    case EncFormat.SHA1: hashType = "SHA1"; break;
                }
            }
            catch
            {
                hashType = "MD5";
            }

            return hashType;
        }
    }

}
