using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Sys.Workflow
{
    public static class XElementHelper
    {
        public static XElement WriteEndElement(this XElement elem)
        {
            elem.CreateWriter().WriteEndElement();

            return elem;
        }

        public static void WriteEndDocument(this XElement elem)
        {
        }

        public static void Close(this XElement elem)
        {
        }

        public static void Flush(this XElement elem)
        {
        }

        public static XElement WriteNamespace(this XElement elem, string prefix, string namespaceUri)
        {
            if (elem.GetNamespaceOfPrefix(prefix).NamespaceName == namespaceUri)
            {
                return elem;
            }

            XNamespace ns = namespaceUri;

            elem.WriteAttribute($"{XNamespace.Xmlns}{prefix}", ns);

            return elem;
        }

        public static XElement WriteDefaultNamespace(this XElement elem, string namespaceUri)
        {
            if (elem.GetPrefixOfNamespace(namespaceUri) == XNamespace.Xmlns)
            {
                return elem;
            }

            XNamespace ns = namespaceUri;

            elem.WriteAttribute($"{XNamespace.Xmlns}", ns);

            return elem;
        }

        public static XComment WriteComment(this XElement elem, string data)
        {
            XComment c = new XComment(data);

            elem.Add(c);

            return c;
        }

        public static XElement WriteEmptyElement(this XElement writer, string prefix, string namespaceURI, string localName)
        {
            return writer.WriteStartElement(prefix, localName, namespaceURI);
        }

        public static XElement WriteEmptyElement(this XElement writer, string localName)
        {
            return writer.WriteStartElement(localName, "");
        }

        public static XElement WriteEmptyElement(this XElement writer, string namespaceURI, string localName)
        {
            return writer.WriteStartElement(localName, namespaceURI);
        }

        public static XElement WriteStartElement(this XElement parent, string localName, string namespaceURI = "")
        {
            var xname = XName.Get($"{localName}", namespaceURI);

            return new XElement(xname);
        }

        public static XElement WriteStartElement(this XElement parent, string prefix, string localName, string namespaceURI = "")
        {
            var xname = XName.Get($"{prefix}:{localName}", namespaceURI);

            return new XElement(xname);
        }

        public static XAttribute WriteAttribute(this XElement element, string namespaceUri, string name, object value)
        {
            var attr = new XAttribute(XName.Get($"{name}", namespaceUri), value);

            element.Add(attr);

            return attr;
        }

        public static XAttribute WriteAttribute(this XElement element, string prefix, string namespaceUri, string name, object value)
        {
            var attr = new XAttribute(XName.Get($"{prefix}{(string.IsNullOrWhiteSpace(prefix) ? "" : ":")}{name}", namespaceUri), value);

            element.Add(attr);

            return attr;
        }

        public static XAttribute WriteAttribute(this XElement element, string name, object value)
        {
            var attr = new XAttribute(name, value);

            element.Add(attr);

            return attr;
        }

        public static XCData WriteCharacters(this XElement parent, string data)
        {
            return parent.WriteCData(data);
        }

        public static XCData WriteCData(this XElement parent, string data)
        {
            var cdata = new XCData(data);

            parent.Add(cdata);

            return cdata;
        }

        public static XElement WriteProcessingInstruction(this XElement elem, string target)
        {
            return elem.WriteProcessingInstruction(target, "");
        }

        public static XElement WriteProcessingInstruction(this XElement elem, string target, string data)
        {
            XProcessingInstruction xpi = new XProcessingInstruction(target, data);

            elem.Add(xpi);

            return elem;
        }

        public static XElement WriteDTD(this XElement elem, string dtd)
        {
            elem.CreateWriter().WriteDocType(dtd, "", "", "");

            return elem;
        }

        public static XElement WriteEntityRef(this XElement elem, string entityRef)
        {
            XEntity entity = new XEntity(entityRef);

            elem.Add(entity);

            return elem;
        }

        public static XElement WriteStartDocument(this XElement elem)
        {
            return elem.WriteStartDocument("");
        }

        public static XElement WriteStartDocument(this XElement elem, string version)
        {
            return elem.WriteStartDocument("utf-8", "");
        }

        public static XElement WriteStartDocument(this XElement elem, string encoding, string version)
        {
            var writer = elem.CreateWriter();

            writer.Settings.Encoding = Encoding.GetEncoding(encoding) ?? Encoding.UTF8;

            writer.WriteStartDocument(true);

            return elem;
        }

        public static XElement WriteCharacters(this XElement elem, char[] text, int start, int len)
        {
            var writer = elem.CreateWriter();

            writer.WriteChars(text, start, len);

            return elem;
        }

        public static string GetPrefix(this XElement elem, string uri)
        {
            return elem.GetPrefixOfNamespace(uri);
        }

        public static XElement SetPrefix(this XElement elem, string prefix, string uri)
        {
            //var writer = elem.CreateWriter();

            return elem;
        }
    }

    public class XEntity : XText
    {
        public override void WriteTo(XmlWriter writer)
        {
            writer.WriteEntityRef(this.Value);
        }

        public XEntity(string value) : base(value) { }
    }
}
