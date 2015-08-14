
namespace Helix.Utility
{
    public static class StringExtensions
    {
        public static string LimitString(this string Value, int limit=64)
        {
            if (Value == null)
                return "";

            if (Value.Length > limit)
                return Value.Substring(0, limit - 5) + "...";

            return Value;
        }

        public static string IsNullOrEmpty(this string val, string def)
        {
            return string.IsNullOrEmpty(val) ? def : val;
        }

        public static bool IsNullOrEmpty(this string val)
        {
            return string.IsNullOrEmpty(val);
        }
    }
}
