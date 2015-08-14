using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml;

namespace Helix.Xml
{
    using System.Reflection;

    /// <summary>
    /// This class allows you to directly navigate XML documents in a similar fashion to XPath using .NET.
    /// </summary>
    /// <example>
    ///     // If you intend to use lambda expressions, you'll need to declare an IPerson interface to 
    ///     // work around some C# limitations.
    ///                 
    ///     // Resolves to: "/*/Contacts/Contact[Name = 'Tony']/Phone[1]/text()"
    ///     string tonyPhoneNumber = addressBook.Contacts.Contact["Name = 'Tony'"].Phone;
    ///     string tonyPhoneNumber2 = addressBook.Contacts.Contact["Name = 'Tony'"].Phone.Value();
    ///     string tonyPhoneNumber3 = addressBook.Contacts.Contact["Name = 'Tony'"].Phone[0].Value();
    ///
    ///     // Resolves to: "/*/Contacts/Contact[Name = 'Steph']/Phone[1]/text()"
    ///     // Currently not in supported in C# 4.0
    ///     // C# 4.0 does not support operations on dynamic types in Expression&lt;TDelegate&gt;
    ///     //string stephPhoneNumber = addressBook.Contacts.Contact[() =&gt; p.Name == 'Steph'].Phone;
    ///     //string stephPhoneNumber2 = addressBook.Contacts.Contact[p =&gt; p.Name == 'Steph'].Phone.Value();
    ///
    ///     // Resolves to: "/*/Contacts/Contact[Name = 'Steph']/Phone[1]/text()"
    ///     // Workaround for the above C# 4.0 limitation using IPerson interface and declaring Expression outside of dynamic object
    ///     Expression&lt;Func&lt;IPerson, bool&gt;&gt; nameEqualsSteph = (p) =&gt; p.Name == "Steph";
    ///     string stephPhoneNumber3 = addressBook.Contacts.Contact[nameEqualsSteph].Phone;
    ///
    ///     // Resolves to: "/*/Contacts/Contact[Name = 'Steph']/Phone[1]/text()"
    ///     // Currently not in supported in C# 4.0
    ///     //List&lt;string&gt; tonyAndStephsPhoneNumbers = addressBook.Contacts.Contact[p =&gt; p.Name == "Tony" || p.Name == 'Steph'].Phone;
    ///
    ///     // Resolves to: "/*/Contacts/Contact[Name = 'Tony' or Name = 'Steph']/Phone[1]/text()"
    ///     // Workaround for the above C# 4.0 limitation.  We use a helper interface (IPerson) to get around some of the limitations.
    ///     // String comparisions (greater than, less than, ...) are supported via the DynamicTools.ComparableString class.
    ///     // There is speculation that C# will support extension operator overloading in a future version.
    ///     Expression&lt;Func&lt;IPerson, bool&gt;&gt; nameEqualsTonyOrSteph = (p) =&gt; p.Name == "Tony" || p.Name == "Steph";
    ///     List&lt;string&gt; tonyAndStephsPhoneNumbers2 = addressBook.Contacts.Contact[nameEqualsTonyOrSteph].Phone;
    ///
    ///     // Resolves to: "/*/Contacts/Contact/Phone"
    ///     XmlNodeList allPhoneNumberNodes = addressBook.Contacts.Contact.Phone;
    ///
    ///     // Resolves to: "/*/Contacts/Contact/Phone/text()"
    ///     List&lt;string&gt; allPhoneNumbers = addressBook.Contacts.Contact.Phone;
    ///     IEnumerable&lt;string&gt; allPhoneNumbers2 = addressBook.Contacts.Contact.Phone.Values();
    ///
    ///     // Resolves to: "/*/Contacts/Contact[1]"
    ///     XmlNode firstContact = addressBook.Contacts.Contact;
    ///     XmlNode firstContact2 = addressBook.Contacts.Contact.Node();
    ///     XmlNode firstContact3 = addressBook.Contacts.Contact[0].Node();
    ///
    ///     // Resolves to: "/*/Contacts/Contact[Name = 'Tony']/Phone[1]/@Tag"
    ///     string phoneTag = addressBook.Contacts.Contact["Name = 'Tony'"].Phone.Attribute("Tag");
    ///
    ///     // Enumerating over the dynamic XPath
    ///     foreach (dynamic contact in addressBook.Contacts.Contact)
    ///     {
    ///         Console.WriteLine(contact.Name);
    ///     }
    /// </example>
    
    public sealed class DynamicXpath : DynamicObject, IEnumerable<DynamicXpath>
    {
        private readonly XmlDocument doc;
        private readonly DynamicXpath parent;
        private readonly string contextStep;
        private WeakReference results;


        /// <summary>
        /// Construct a dynamic XPath based on the specified XML document
        /// </summary>
        /// <param name="doc">The XML document to query</param>
        private DynamicXpath(XmlDocument doc)
            : this(null, Step.Root(doc))
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc", "Document cannot be null");
            }

            this.doc = doc;
        }

        /// <summary>
        /// Construct a dynamic XPath based on the specified XML element
        /// </summary>
        /// <param name="element">The XML element to start query from</param>
        private DynamicXpath(XmlElement element)
            : this(null, Step.Root(element))
        {
            if (element == null)
            {
                throw new ArgumentNullException("element", "Element cannot be null");
            }

            this.doc = element.OwnerDocument;
        }

        /// <summary>
        /// Construct a child dynamic XPath belonging to the specified parent.
        /// </summary>
        /// <param name="parent">The parent dyanmic XPath</param>
        /// <param name="step">The XPath step</param>
        private DynamicXpath(DynamicXpath parent, string step)
        {
            this.parent = parent;
            this.contextStep = step;

            if (parent != null)
            {
                this.doc = parent.doc;
            }
        }

        /// <summary>
        /// Creates a child selector.  The index is zero-based.
        /// </summary>
        /// <param name="index">The index of the child</param>
        /// <returns>The resulting dynamic XPath</returns>
        public DynamicXpath this[int index]
        {
            get
            {
                return new DynamicXpath(this, Step.Index(index));
            }
        }

        /// <summary>
        /// Create a predicate
        /// </summary>
        /// <param name="predicate">The predicate string</param>
        /// <returns>The resulting dynamic XPath</returns>
        public DynamicXpath this[string predicate]
        {
            get
            {
                return new DynamicXpath(this, Step.Predicate(predicate));
            }
        }

        /// <summary>
        /// Creates an XPath predicate based on the specified predicate expression.
        /// Currently not supported by C# 4.0.
        /// </summary>
        /// <remarks>
        /// Maybe dynamic types will be supported in labda expression in future C# release.
        /// </remarks>
        /// <param name="predicate">A binary Expression</param>
        /// <returns>The resulting dynamic XPath</returns>
        public DynamicXpath this[LambdaExpression predicate]
        {
            get
            {
                return new DynamicXpath(this, Step.Predicate(predicate));
            }
        }

        /// <summary>
        /// Creates an XPath predicate based on the specified example objects.
        /// </summary>
        /// <param name="exampleObjects">Example objects to match</param>
        /// <returns>The resulting dynamic XPath</returns>
        public DynamicXpath this[params object[] exampleObjects]
        {
            get
            {
                Example[] examples = exampleObjects.Select(e => new Example(e)).ToArray();
                return this[examples];
            }
        }

        /// <summary>
        /// Creates an XPath predicate based on the specified example.
        /// </summary>
        /// <param name="examples">Example to match</param>
        /// <returns>The resulting dynamic XPath</returns>
        public DynamicXpath this[params Example[] examples]
        {
            get
            {
                return new DynamicXpath(this, Step.Predicate(examples));
            }
        }

        /// <summary>
        /// Create a dyanic XPath to query the specified document
        /// </summary>
        /// <param name="doc">The document to create DynamicXPath from</param>
        /// <returns>A dynamic XPath to query the specified document</returns>
        public static dynamic Load(XmlDocument doc)
        {
            return new DynamicXpath(doc);
        }

        /// <summary>
        /// Create a dyanic XPath to query the specified element
        /// </summary>
        /// <param name="element">The element to create DynamicXPath from</param>
        /// <returns>A dynamic XPath to query the specified element</returns>
        public static dynamic Load(XmlElement element)
        {
            return new DynamicXpath(element);
        }

        /// <summary>
        /// Create a dyanic XPath to query the specified file
        /// </summary>
        /// <param name="filename">The file to query</param>
        /// <returns>A dynamic XPath to query the specified file</returns>
        public static dynamic Load(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            return new DynamicXpath(doc);
        }

        /// <summary>
        /// Create a dyanic XPath to query the specified xml data
        /// </summary>
        /// <param name="xml">The xml data query</param>
        /// <returns>A dynamic XPath to query the specified xml data</returns>
        public static dynamic LoadXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return new DynamicXpath(doc);
        }

        /// <summary>
        /// Get the document
        /// </summary>
        /// <returns></returns>
        public XmlDocument GetDocument()
        {
            return this.doc;
        }

        /// <summary>
        /// Get the root dyanamic XPath
        /// </summary>
        /// <returns>A dynamic XPath that starts at the root of the document</returns>
        public DynamicXpath GetRoot()
        {
            return new DynamicXpath(this.doc);
        }

        /// <summary>
        /// Used internally.  Do not call this method.
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new DynamicXpath(this, Step.Child(binder.Name));
            return true;
        }


        /// <summary>
        /// Get the generated XPath
        /// </summary>
        /// <returns>The XPath statement represented by the dyanmic XPath</returns>
        public string ToXpath()
        {
            Stack<DynamicXpath> nodes = new Stack<DynamicXpath>();
            DynamicXpath cur = this;

            while (cur != null)
            {
                nodes.Push(cur);
                cur = cur.parent;
            }

            StringBuilder buf = new StringBuilder();

            foreach (var node in nodes)
            {
                buf.Append(node.contextStep);
            }

            return buf.ToString();
        }

        /// <summary>
        /// Get the nodes matched by this dynamic XPath
        /// </summary>
        /// <returns>The matching nodes</returns>
        public XmlNodeList Nodes()
        {
            XmlNodeList nodes = null;

            if (this.results != null)
            {
                nodes = this.results.Target as XmlNodeList;
            }

            if (nodes == null)
            {
                nodes = this.doc.SelectNodes(this.ToXpath());
                this.results = new WeakReference(nodes);
            }

            return nodes;
        }

        /// <summary>
        /// Get the first node matched by this dynamic XPath
        /// </summary>
        /// <returns>The first node matching this dynamic XPath</returns>
        public XmlNode Node()
        {
            XmlNodeList nodes = this.Nodes();
            return (nodes != null && nodes.Count > 0) ? nodes[0] : null;
        }

        /// <summary>
        /// Get the attributes with the given name for all matching nodes
        /// </summary>
        /// <param name="attributeName">The attribute name</param>
        /// <returns>All the attributes with the given name for all the matched nodes</returns>
        public IEnumerable<string> Attributes(string attributeName)
        {
            DynamicXpath xpath = new DynamicXpath(this, Step.Attribute(attributeName));
            XmlNodeList nodes = xpath.Nodes();

            if (nodes != null)
            {
                foreach (XmlAttribute attribute in nodes)
                {
                    if (attribute != null)
                    {
                        yield return attribute.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the attribute with the given name for the first node matched by this dynamic XPath
        /// </summary>
        /// <param name="attributeName">The attribute name</param>
        /// <returns>The attribute with the given name for the first node matched by this dynamic XPath</returns>
        public string Attribute(string attributeName)
        {
            XmlNode node = this.Node();

            if (node != null)
            {
                XmlAttribute attribute = node.Attributes[attributeName];

                if (attribute != null)
                {
                    return attribute.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the value for the first node matched by this dynamic XPath
        /// </summary>
        /// <returns>The value for the first node matched by this dynamic XPath</returns>
        public string Value()
        {
            XmlNode node = this.Node();

            if (node != null)
            {
                return (node is XmlElement) ? node.InnerText : node.Value;
            }

            return null;
        }

        /// <summary>
        /// Gets the value in the specified type for the first node matched by this dynamic XPath
        /// </summary>
        /// <typeparam name="TResult">The return type</typeparam>
        /// <returns>The value in the specified type for the first node matched by this dynamic XPath</returns>
        public TResult Value<TResult>()
        {
            string value = this.Value();
            TResult result = default(TResult);

            if (value != null)
            {
                result = (TResult)Convert.ChangeType(value, typeof(TResult));
            }

            return result;
        }

        /// <summary>
        /// Gets the values for the nodes matched by this dynamic XPath
        /// </summary>
        /// <returns>The values for the nodes matched by this dynamic XPath</returns>
        public IEnumerable<string> Values()
        {
            foreach (XmlNode node in this.Nodes())
            {
                yield return (node is XmlElement) ? node.InnerText : node.Value;
            }
        }

        /// <summary>
        /// Gets the values in the specified type for the nodes matched by this dynamic XPath
        /// </summary>
        /// <typeparam name="TResult">The return type</typeparam>
        /// <returns>The values in the specified type for the nodes matched by this dynamic XPath</returns>
        public IEnumerable<TResult> Values<TResult>()
        {
            Type resultType = typeof(TResult);

            foreach (string data in this.Values())
            {
                TResult result = (TResult)Convert.ChangeType(data, resultType);
                yield return result;
            }
        }

        /// <summary>
        /// Get the number of nodes in the node list
        /// </summary>
        /// <returns>The number of nodes in the node list</returns>
        public int Count()
        {
            return this.Nodes().Count;
        }

        /// <summary>
        /// Implicity convert dynamic XPath to a string
        /// </summary>
        /// <param name="xpath">The dynamic xpath</param>
        /// <returns>The value of the first node matching the dynamic xpath</returns>
        public static implicit operator string(DynamicXpath xpath)
        {
            return xpath.Value();
        }

        /// <summary>
        /// Implicity convert dynamic XPath to its list of matching string values
        /// </summary>
        /// <param name="xpath">The dynamic xpath</param>
        /// <returns>The list of values for the nodes matching the dynamic xpath</returns>
        public static implicit operator List<string>(DynamicXpath xpath)
        {
            return xpath.Values().ToList();
        }

        /// <summary>
        /// Implicity convert dynamic XPath an XML node list
        /// </summary>
        /// <param name="xpath">The dynamic xpath</param>
        /// <returns>The nodes matching the dynamic xpath</returns>
        public static implicit operator XmlNodeList(DynamicXpath xpath)
        {
            return xpath.Nodes();
        }

        /// <summary>
        /// Implicity convert dynamic XPath an XML node
        /// </summary>
        /// <param name="xpath">The dynamic xpath</param>
        /// <returns>The first node matching the dynamic xpath</returns>
        public static implicit operator XmlNode(DynamicXpath xpath)
        {
            return xpath.Node();
        }


        #region IEnumerable<DynamicXpath> Members

        /// <summary>
        /// Get enumerator to enumerate over all the dynamic XPaths from xpath[0] to xpath[n - 1].
        /// </summary>
        /// <returns>An enumerator to enumerate over all the dynamic XPaths</returns>
        public IEnumerator<DynamicXpath> GetEnumerator()
        {
            XmlNodeList nodes = this.Nodes();

            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    yield return new DynamicXpath(this, Step.Index(i));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }




    /// <summary>
    /// XPath step generator
    /// </summary>
    internal static class Step
    {
        public static string Root(XmlDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }

            return string.Format("/{0}", doc.DocumentElement.Name);
        }

        public static string Root(XmlElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Stack<XmlNode> ancestors = new Stack<XmlNode>();

            XmlNode cur = element;

            while ((cur != null) && !(cur is XmlDocument))
            {
                ancestors.Push(cur);
                cur = cur.ParentNode;
            }

            StringBuilder buf = new StringBuilder();

            foreach (XmlNode node in ancestors)
            {
                buf.Append(Step.Child(node.Name));
            }

            return buf.ToString();
        }

        public static string Child(string elementName)
        {
            if (string.IsNullOrWhiteSpace(elementName))
            {
                throw new ArgumentException("Element name can't be null or empty");
            }

            return string.Format("/{0}", elementName);
        }

        public static string Attribute(string attributeName)
        {
            if (string.IsNullOrWhiteSpace(attributeName))
            {
                throw new ArgumentException("Attribute name can't be null or empty");
            }

            return string.Format("/@{0}", attributeName);
        }

        public static string Predicate(string predicate)
        {
            if (string.IsNullOrWhiteSpace(predicate))
            {
                throw new ArgumentException("Predicate name can't be null or empty");
            }

            return string.Format("[{0}]", predicate);
        }

        public static string Predicate(params Example[] examples)
        {
            if (examples == null || examples.Length == 0)
            {
                throw new ArgumentException("Predicate examples can't be null or empty");
            }

            StringBuilder buf = new StringBuilder("[");
            bool wrapExpressionsInParenthesis = examples.Length > 1;

            for (int i = 0; i < examples.Length; i++)
            {
                Example example = examples[i];

                if (i > 0)
                {
                    buf.Append(" or ");
                }

                buf.Append(ConvertExampleToExpression(example, wrapExpressionsInParenthesis));
            }

            buf.Append("]");
            return buf.ToString();
        }

        private static string ConvertExampleToExpression(Example example, bool wrap)
        {
            StringBuilder buf = new StringBuilder();

            if (wrap)
            {
                buf.Append("(");
            }

            BindingFlags bindings = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.DeclaredOnly | BindingFlags.Instance;
            PropertyInfo[] properties = example.ExampleObject.GetType().GetProperties(bindings);

            if (properties.Length == 0)
            {
                throw new ArgumentException("Example does not contain any properties.  Can't convert to expression.");
            }

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                object value = property.GetValue(example.ExampleObject, null);

                if (i > 0)
                {
                    buf.Append(" and ");
                }

                if (example.IsAttribute)
                {
                    buf.Append("@");
                }

                buf.AppendFormat("{0} = {1}", property.Name, XpathHelper.EscapeXpathValue(value));
            }

            if (wrap)
            {
                buf.Append(")");
            }

            return buf.ToString();
        }

        public static string Index(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", "path index must at least zero");
            }

            return string.Format("[{0}]", (index + 1));
        }

        public static string Predicate(LambdaExpression exp)
        {
            if (exp == null)
            {
                throw new ArgumentNullException("exp", "Expression can not be null");
            }

            string predicate = XpathExpressionParser.Expression(exp);
            return Step.Predicate(predicate);
        }
    }

    /// <summary>
    /// Wraps example objects
    /// </summary>
    public class Example
    {
        private readonly object exampleObject;
        private readonly bool isAttribute;

        /// <summary>
        /// Contructs an Element example object
        /// </summary>
        /// <param name="exampleObject">The example object</param>
        public Example(object exampleObject)
            : this(exampleObject, false)
        {
        }

        /// <summary>
        /// Constructs an example object.  If <c>isAttribute</c> is <c>false</c>, then constructs an Element example object;
        /// otherwise constructs an Attribute example object
        /// </summary>
        /// <param name="exampleObject">The example object</param>
        /// <param name="isAttribute">If false, then constructs an Element example object;  otherwise constructs an Attribute example object</param>
        protected Example(object exampleObject, bool isAttribute)
        {
            if (exampleObject == null)
            {
                throw new ArgumentNullException("exampleObject", "Example object can't be null.");
            }

            this.exampleObject = exampleObject;
            this.isAttribute = isAttribute;
        }

        /// <summary>
        /// Get the example object
        /// </summary>
        public object ExampleObject
        {
            get
            {
                return this.exampleObject;
            }
        }

        /// <summary>
        /// Get whether the properties of the example object represents elements or attributes.
        /// If <c>true</c> then the wrapped object properties represent attributes, otherwise, they represent elements.
        /// </summary>
        public bool IsAttribute
        {
            get
            {
                return isAttribute;
            }
        }
    }

    internal static class XpathExpressionParser
    {
        public static string Expression(LambdaExpression exp)
        {
            string predicate = Expression(exp.Body);
            return predicate;
        }

        private static string Expression(Expression exp)
        {
            if (exp is BinaryExpression)
            {
                return BinaryExpression(exp as BinaryExpression);
            }
            else if (exp is MemberExpression)
            {
                return MemberExpression(exp as MemberExpression);
            }
            else if (exp is ConstantExpression)
            {
                return ConstantExpression(exp as ConstantExpression);
            }
            else if (exp is UnaryExpression)
            {
                return UnaryExpression(exp as UnaryExpression);
            }

            throw new NotSupportedException(string.Format("Expression is not supported: {0}", exp.GetType().FullName));
        }

        private static string UnaryExpression(UnaryExpression exp)
        {
            if (exp == null)
            {
                throw new ArgumentNullException("exp");
            }

            if (exp.NodeType == ExpressionType.Convert)
            {
                return Expression(exp.Operand);
            }
            else if (exp.Method.GetParameters().Length == 0)
            {
                object obj = exp.Method.Invoke(exp.Operand, null);
                return Quote(obj);
            }

            throw new NotSupportedException();
        }

        private static string ConstantExpression(ConstantExpression exp)
        {
            if (exp == null)
            {
                throw new ArgumentNullException("exp");
            }

            return Quote(exp.Value);
        }

        private static string MemberExpression(MemberExpression exp)
        {
            if (exp == null)
            {
                throw new ArgumentNullException("exp");
            }

            return exp.Member.Name;
        }

        private static string GetPredicateOperator(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.AndAlso:
                    return "and";
                case ExpressionType.OrElse:
                    return "or";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "div";
                default:
                    throw new NotSupportedException(string.Format("The given operator expression is not supported: {0}", nodeType.ToString()));
            }
        }

        private static string BinaryExpression(BinaryExpression exp)
        {
            if (exp == null)
            {
                throw new ArgumentNullException("exp");
            }

            string op = GetPredicateOperator(exp.NodeType);
            string left = Expression(exp.Left);
            string right = Expression(exp.Right);
            return string.Format("{0} {1} {2}", left, op, right);
        }

        private static string Quote(object obj)
        {
            string value;
            TypeCode typeCode = Convert.GetTypeCode(obj);

            switch (typeCode)
            {
                    // Simple Quote
                case TypeCode.Char:
                    value = string.Format("'{0}'", obj != null ? obj.ToString() : string.Empty);
                    break;

                    // Quote
                case TypeCode.Object:
                case TypeCode.String:
                case TypeCode.DateTime:
                    value = XpathHelper.EscapeXpathValue(obj);
                    break;

                case TypeCode.Boolean:
                    value = (Convert.ToBoolean(obj)) ? "true" : "false";
                    break;

                    // NO quote
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    value = Convert.ToString(obj);
                    break;

                case TypeCode.DBNull:
                case TypeCode.Empty:
                    value = "''";
                    break;
                default:
                    throw new NotSupportedException(string.Format("Type not supported: {0}", typeCode.ToString()));
            }

            return value;
        }
    }

    internal static class XpathHelper
    {
        public static readonly string DateFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

        public static string EscapeXpathValue(object value)
        {
            string text = value != null ? value.ToString() ?? string.Empty : string.Empty;

            if (value is DateTime)
            {
                DateTime dt = (DateTime)value;
                return string.Format("'{0}'", FormatDate(dt));
            }
            else if (value is DateTime?)
            {
                DateTime? dt = value as DateTime?;
                return string.Format("'{0}'", dt.HasValue ? FormatDate(dt.Value) : string.Empty);
            }
            else if (IsNumeric(text))
            {
                return text;
            }
            else if (IsBoolean(text))
            {
                return (Convert.ToBoolean(text)) ? "true" : "false";
            }
            else if (!text.Contains("'"))
            {
                return string.Format("'{0}'", text);
            }
            else if (!text.Contains("\""))
            {
                return string.Format("\"{0}\"", text);
            }
            else
            {
                StringBuilder buf = new StringBuilder("concat(");
                string[] tokens = text.Split('\'');
                for (int i = 0; i < tokens.Length; i++)
                {
                    string token = tokens[i];

                    if (i > 0)
                    {
                        buf.Append(",\"'\",");
                    }

                    buf.AppendFormat("\"{0}\"", token);
                }

                buf.Append(")");
                return buf.ToString();
            }
        }

        private static bool IsNumeric(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            double result;
            return double.TryParse(s, out result);
        }

        private static bool IsBoolean(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            bool result;
            return bool.TryParse(s, out result);
        }

        private static string FormatDate(DateTime date)
        {
            return date.ToUniversalTime().ToString(DateFormat);
        }
    }

    /// <summary>
    /// Wraps example object for attribute
    /// </summary>
    public class AttributeExample : Example
    {
        /// <summary>
        /// Constructs an example object where the object's properties represent XML attributes
        /// </summary>
        /// <param name="exampleObject">The example object</param>
        public AttributeExample(object exampleObject)
            : base(exampleObject, true)
        {
        }
    }
}

