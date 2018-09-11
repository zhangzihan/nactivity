using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Sys
{
    public static class XExtensions
    {
        public static string GetAttribute(this XNode node, string attrName, string namespaceURI = "")
        {
            if (node is XElement elem)
            {
                return elem.Attribute($"{namespaceURI}{attrName}")?.Value;
            }

            return null;
        }

        public static IEnumerable<XAttribute> GetAttributes(this XNode node, XName name = null)
        {
            if (node is XElement elem)
            {
                return elem.Attributes(name);
            }

            return null;
        }

        public static string GetName(this XObject xo)
        {
            if (xo is XElement elem)
                return elem.Name.LocalName;
            if (xo is XAttribute attr)
                return attr.Name.LocalName;

            switch (xo.NodeType)
            {
                case XmlNodeType.CDATA:
                    return "#cdata-section";
                case XmlNodeType.Text:
                case XmlNodeType.Whitespace:
                    return "#text";
                case XmlNodeType.Comment:
                    return "#comment";
                default:
                    return null;
            }
        }

        public static string GetValue(this XObject node)
        {
            if (node is XElement ell)
            {
                if (!ell.Elements().Any())
                    return ell.Value;
            }

            if (node is XAttribute att)
            {
                return att.Value;
            }

            if (node is XNode o && !o.CreateNavigator().HasChildren)
            {
                return o.CreateNavigator().Value;
            }

            return "";
        }

        public delegate T XObjectProjectionFunc<T>(object o, int depth);


        public delegate T XElementProjectionFunc<T>(XElement o, int depth);

        public static IEnumerable<T> Visit<T>(this XElement source, XObjectProjectionFunc<T> func)
        {
            foreach (var v in Visit(source, func, 0))
                yield return v;
        }



        public static IEnumerable<T> Visit<T>(XElement source, XObjectProjectionFunc<T> func, int depth)
        {
            yield return func(source, depth);
            foreach (XAttribute att in source.Attributes())
                yield return func(att, depth + 1);
            foreach (XElement child in source.Elements())
                foreach (T s in Visit(child, func, depth + 1))
                    yield return s;
        }



        public delegate void XObjectVisitor(object o, int depth);



        public static void Visit(this XElement source, XObjectVisitor func)
        {
            Visit(source, func, 0);
        }



        public static void Visit(XElement source, XObjectVisitor func, int depth)
        {
            func(source, depth);
            foreach (XAttribute att in source.Attributes())
                func(att, depth + 1);
            foreach (XElement child in source.Elements())
                Visit(child, func, depth + 1);
        }



        public static IEnumerable<T> VisitElements<T>(this XElement source, XElementProjectionFunc<T> func)
        {
            foreach (var v in VisitElements(source, func, 0))
                yield return v;
        }



        public static IEnumerable<T> VisitElements<T>(XElement source, XElementProjectionFunc<T> func, int depth)
        {
            yield return func(source, depth);
            foreach (XElement child in source.Elements())
                foreach (T s in VisitElements(child, func, depth + 1))
                    yield return s;
        }



        public delegate void XElementVisitor(XElement o, int depth);



        public static void VisitElements(this XElement source, XElementVisitor func)
        {
            VisitElements(source, func, 0);
        }



        public static void VisitElements(XElement source, XElementVisitor func, int depth)
        {
            func(source, depth);
            foreach (XElement child in source.Elements())
                VisitElements(child, func, depth + 1);
        }



        public delegate void XNodeVisitor(XNode o, int depth);

        public static void VisitNodes(this XContainer source, XNodeVisitor func)
        {
            VisitNodes(source, func, 0);
        }

        public static void VisitNodes(XContainer source, XNodeVisitor func, int depth)
        {
            func(source, depth);
            foreach (XNode child in source.Nodes())
            {
                if (child is XElement)
                {
                    VisitNodes(child as XContainer, func, depth + 1);
                }
                else
                {
                    func(child, depth + 1);
                }
            }
        }
    }
}
