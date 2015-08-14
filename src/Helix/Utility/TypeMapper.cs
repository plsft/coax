using System.Collections;

namespace Helix.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Dynamic;
    using System.Reflection;

    public static class TypeMapper<T> where T : class
    {
        private static readonly Dictionary<string, PropertyInfo> _propertyMap;
        private static object o; 

        static TypeMapper()
        {
            _propertyMap = typeof (T).GetProperties().ToDictionary(p => p.Name.ToLower(), p => p);
            o = new object(); 
        }

        public static dynamic ToExpando(object o)
        {
            lock (o)
            {
                var result = new ExpandoObject();
                var d = result as IDictionary<string, object>;

                if (o is ExpandoObject)
                    return o;

                if (o.GetType() == typeof (NameValueCollection) || o.GetType().IsSubclassOf(typeof (NameValueCollection)))
                {
                    var nv = (NameValueCollection) o;
                    nv.Cast<string>().Select(key => new KeyValuePair<string, object>(key, nv[key])).ToList().ForEach(i => d.Add(i));
                }
                else
                {
                    var props = o.GetType().GetProperties();
                    foreach (var item in props)
                    {
                        d.Add(item.Name, item.GetValue(o, null));
                    }
                }

                return result;
            }
        }

        public static dynamic MapToDynamic(NameValueCollection coll)
        {
            return ToExpando(coll);
        }

        /// <summary>
        /// Maps name value collection to type T
        /// </summary>
        /// <param name="coll">NV pairs</param>
        /// <param name="destination">T object</param>
        public static void Map(NameValueCollection coll, T destination)
        {
            Map(ToExpando(coll), destination);
        }

        /// <summary>
        /// Maps dynamic object to Expando, then T
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void Map(dynamic source, T destination)
        {
            Map(ToExpando(source), destination);
        }


        /// <summary>
        /// Maps ExpandoObject to T
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void Map(ExpandoObject source, T destination)
        {
            lock (o)
            {
                if (source == null)
                    throw new ArgumentNullException("source");

                if (destination == null)
                    throw new ArgumentNullException("destination");

                foreach (var kv in source)
                {
                    PropertyInfo p;
                    if (_propertyMap.TryGetValue(kv.Key.ToLower(), out p))
                    {
                        var propType = p.PropertyType;
                        if (kv.Value == null)
                        {
                            if (kv.Value != null && !propType.IsByRef && propType.Name != "Nullable`1")
                                throw new ArgumentException("[" + kv.Key + "] key with value [" + kv.Value + "] is not nullable but null value detected! Consider retyping property as " + kv.Key +
                                                            "? in POCO.");
                        }
                        else if ((kv.Value != null) && kv.Value.GetType() != propType && propType.Name != "Nullable`1")
                            throw new ArgumentException(kv.Key + " key with value [" + kv.Value + "] type mismatch to [" + propType.FullName + "]");

                        if ((kv.Value != null) && kv.Value.GetType() != typeof (IEnumerable) || (kv.Value != null) && kv.Value.GetType() != typeof (IList))
                            p.SetValue(destination, kv.Value, null);
                    }
                }
            }
        }
    }
}

