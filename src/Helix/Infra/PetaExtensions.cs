using System;

namespace Helix.Infra
{
    public static class PetaExtensions
    {
        /// <summary>
        /// Assumes default values are "" (string), null for all other types
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool HasValue(this object f)
        {
            if (f == null)
                return false;

            switch (f.GetType().Name.ToLower())
            {
                case "string": return !string.IsNullOrEmpty(Convert.ToString(f));
                case "datetime": return Convert.ToDateTime(f) > DateTime.MinValue;

                default: return true;
            }
        }
    }
}
