using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coax.General
{
    public static class ConfigExtensionMethods
    {
        public static string DefaultValue(this string val, string defaultReturnedValue)
        {
            return string.IsNullOrEmpty(val) ? defaultReturnedValue : Convert.ToString(val);
        }
    }
}
