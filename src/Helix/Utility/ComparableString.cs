
namespace Helix.Utility
{
    /// <summary>
    /// A string wrapper class that provides comparisons of strings with operators.
    /// This gives us the ability to use operator comparisons on strings in lambda expressions for DynamicXpath predicates.
    /// </summary>
    /// <remarks>
    /// Microsoft has hinted at providing extension operator overloading and extension properties in future versions of C#.
    /// This will reduce the need for this wrapper class.
    /// </remarks>
    /// <example>
    /// (person) => person.Name >= "C"
    /// </example>
    public sealed class ComparableString
    {
        private readonly string data;

        /// <summary>
        /// Wrap the given string
        /// </summary>
        /// <param name="str">The wrapped string</param>
        public ComparableString(string str)
        {
            this.data = str;
        }

        /// <summary>
        /// The wrapped string
        /// </summary>
        public string Data
        {
            get { return this.data; }
        }

        /// <summary>
        /// Get the string encapsulated by this object
        /// </summary>
        /// <returns>The wrapped string</returns>
        public override string ToString()
        {
            return this.data;
        }

        /// <summary>
        /// Returns true if the given object is a string equal to the wrapped string or if the 
        /// object is a ComparableString that wraps the same string.
        /// </summary>
        /// <param name="obj">The object to test equality against</param>
        /// <returns>true if the given object is a string equal to the wrapped string or if the 
        /// object is a ComparableString that wraps the same string</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (object.ReferenceEquals(obj, this))
            {
                return true;
            }
            else if (obj is string)
            {
                return this.data == obj as string;
            }

            ComparableString cs = obj as ComparableString;
            if (!object.Equals(cs, null))
            {
                return object.Equals(this.data, cs.data);
            }

            return false;
        }

        /// <summary>
        /// Get the hash code of the wrapped string
        /// </summary>
        /// <returns>the hash code of the wrapped string</returns>
        public override int GetHashCode()
        {
            return this.data.GetHashCode();
        }

        /// <summary>
        /// Implicitly convert the ComparableString to a System.String.
        /// </summary>
        /// <param name="c">The ComparableString to convert</param>
        /// <returns>The string equivalent of the given ComparableString</returns>
        public static implicit operator string(ComparableString c)
        {
            return (c != null) ? c.data : null;
        }

        /// <summary>
        /// Implicitly convert the specified string to a ComparableString.
        /// </summary>
        /// <param name="str">The System.String to convert</param>
        /// <returns>The ComparableString equivalent of the given string</returns>
        public static implicit operator ComparableString(string str)
        {
            return new ComparableString(str);
        }

        /// <summary>
        /// Determine if the ComparableStrings have the same value
        /// </summary>
        /// <param name="a">The first ComparableString</param>
        /// <param name="b">The second ComparableString</param>
        /// <returns>true if the values the same; otherwise, false.</returns>
        public static bool operator ==(ComparableString a, ComparableString b)
        {
            return object.Equals(a, b);
        }

        /// <summary>
        /// Determine if the ComparableStrings have different values
        /// </summary>
        /// <param name="a">The first ComparableString</param>
        /// <param name="b">The second ComparableString</param>
        /// <returns>true if the values are different; otherwise, false.</returns>
        public static bool operator !=(ComparableString a, ComparableString b)
        {
            return !object.Equals(a, b);
        }

        /// <summary>
        /// Determine if the first ComparableString is greater than the second ComparableString
        /// </summary>
        /// <param name="a">The first ComparableString</param>
        /// <param name="b">The second ComparableString</param>
        /// <returns>true if a is greater than b; otherwise, false.</returns>
        public static bool operator >(ComparableString a, ComparableString b)
        {
            if (object.Equals(a, null))
            {
                return false;
            }
            else if (object.Equals(b, null))
            {
                return true;
            }

            return a.data.CompareTo(b.data) > 0;
        }

        /// <summary>
        /// Determine if the first ComparableString is greater than or equal to the second ComparableString
        /// </summary>
        /// <param name="a">The first ComparableString</param>
        /// <param name="b">The second ComparableString</param>
        /// <returns>true if a is greater than or equal b; otherwise, false.</returns>
        public static bool operator >=(ComparableString a, ComparableString b)
        {
            if (object.Equals(a, null))
            {
                return object.Equals(b, null);
            }

            return a.data.CompareTo(b.data) >= 0;
        }

        /// <summary>
        /// Determine if the first ComparableString is less than to the second ComparableString
        /// </summary>
        /// <param name="a">The first ComparableString</param>
        /// <param name="b">The second ComparableString</param>
        /// <returns>true if a is less than b; otherwise, false.</returns>
        public static bool operator <(ComparableString a, ComparableString b)
        {
            if (object.Equals(a, null))
            {
                return !object.Equals(b, null);
            }
            else if (object.Equals(b, null))
            {
                return false;
            }

            return a.data.CompareTo(b.data) < 0;
        }

        /// <summary>
        /// Determine if the first ComparableString is less than or equal to the second ComparableString
        /// </summary>
        /// <param name="a">The first ComparableString</param>
        /// <param name="b">The second ComparableString</param>
        /// <returns>true if a is less than or equal b; otherwise, false.</returns>
        public static bool operator <=(ComparableString a, ComparableString b)
        {
            if (object.Equals(a, null))
            {
                return true;
            }

            return a.data.CompareTo(b.data) <= 0;
        }
    }
}
