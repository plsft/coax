
namespace Helix.Utility
{
    using System.Collections;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class Strings
    {
        /// <summary>
        /// Remove Html
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string RemoveHtml(string html)
        {
            //var reg = new Regex("<.*?>", RegexOptions.Compiled); 
            //return reg.Replace(html, string.Empty);
            // 70% faster to use compiled reg ex.
            return new Regex("<.*?>", RegexOptions.Compiled).Replace(html, string.Empty); // use only one line. 
        }

 
        /// <summary>
        /// Sql Safe String
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static string SqlSafeString(string sql)
        {
            return sql.Replace("'", "''").Replace("--", "");
        }

        /// <summary>
        /// Convert to XML
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitchar"></param>
        /// <param name="valuepairsplitchar"></param>
        /// <param name="removeHtml"></param>
        /// <returns></returns>
        public static string Xmlify(string str, char splitchar, char valuepairsplitchar, bool removeHtml)
        {
            str = removeHtml ? RemoveHtml(str).Trim() : str.Trim();
            var splitted = str.Split(splitchar);
            
            var xml = splitted.Where(s => !string.IsNullOrEmpty(s) && s.Contains(valuepairsplitchar.ToString())).Aggregate("<root>", (current, s) => current + ("<" + RemoveHtml(s.Split(valuepairsplitchar)[0]).Trim().Replace(" ", "") + ">" + RemoveHtml(s.Split(valuepairsplitchar)[1]).Trim() + "</" + RemoveHtml(s.Split(valuepairsplitchar)[0]).Trim().Replace(" ", "") + ">"));
            
            xml += "</root>";

            return xml;
        }

        /// <summary>
        /// Convert To NV Pairs
        /// </summary>
        /// <param name="nv"></param>
        /// <returns></returns>
        public static string NameValueCollectionToString(NameValueCollection nv)
        {
            return string.Join("&", (from string name in nv select string.Concat(name, "=", nv[name])).ToArray());
        }

        /// <summary>
        /// Convert Array to CSV
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ConvertArrayToCsv(ArrayList list)
        {
            var retVal = "";
            foreach (var t in list) retVal = t + ",";
            
            if (retVal.EndsWith(","))
                retVal = retVal.Remove(retVal.Length - 1, 1);

            return retVal;

        }

        /// <summary>
        /// Convert Array to PSV
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToPipeList(List<int> list)
        {
            var retvalue = list.Aggregate("", (current, num) => current + (Convert.ToString(num) + "|"));

            if (retvalue.EndsWith("|"))
                retvalue = retvalue.Remove(retvalue.Length - 1, 1);

            return retvalue;
        }
    }
}
