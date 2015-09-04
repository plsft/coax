using System;
using System.Globalization;

namespace Helix.Utility
{
    public sealed class General
    {

        /// <summary>
        /// Random code; default length=8;
        /// </summary>
        /// <returns></returns>
        public static string GenerateRandomCode()
        {
            return GenerateRandomCode(8);
        }

        /// <summary>
        /// Returns Random
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateRandomCode(int length)
        {
            return (Guid.NewGuid().ToString().Replace("-", "") + "" + new Random().Next(128)).ToLower().ToString(CultureInfo.InvariantCulture).Substring(0, length);
        }

        /// <summary>
        /// Returns random password
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateRandomPassword(int length)
        {
            return Guid.NewGuid().ToString().Substring(0, length).Replace("-", "");
        }

        /// <summary>
        /// Returns 32-length unique identifier (format: 00000000000000000000000000000000) 
        /// </summary>
        /// <returns></returns>
        public static string GenerateRandomUniqueId()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Returns 64-length unique identifier (format: 0000000000000000000000000000000000000000000000000000000000000000) 
        /// </summary>
        /// <returns></returns>
        public static string GenerateRandomUniqueId64()
        {
            return Guid.NewGuid().ToString("N") + "" + Guid.NewGuid().ToString("N");
        }

        public static long GenerateTicks()
        {
            return DateTime.Now.Ticks;
        }
    }
}
