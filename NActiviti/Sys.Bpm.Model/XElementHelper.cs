using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Sys.Bpm
{
    public static class XElementHelper
    {
        public static XElement writeEndElement(this XElement elem)
        {
            elem.CreateWriter().WriteEndElement();

            return elem;
        }

        public static void writeEndDocument(this XElement elem)
        {
        }

        public static void close(this XElement elem)
        {
        }

        public static void flush(this XElement elem)
        {
        }

        public static XElement writeNamespace(this XElement elem, string prefix, string namespaceUri)
        {
            if (elem.GetNamespaceOfPrefix(prefix).NamespaceName == namespaceUri)
            {
                return elem;
            }

            XNamespace ns = namespaceUri;

            elem.writeAttribute($"{XNamespace.Xmlns}{prefix}", ns);

            return elem;
        }

        public static XElement writeDefaultNamespace(this XElement elem, string namespaceUri)
        {
            if (elem.GetPrefixOfNamespace(namespaceUri) == XNamespace.Xmlns)
            {
                return elem;
            }

            XNamespace ns = namespaceUri;

            elem.writeAttribute($"{XNamespace.Xmlns}", ns);

            return elem;
        }

        public static XComment writeComment(this XElement elem, string data)
        {
            XComment c = new XComment(data);

            elem.Add(c);

            return c;
        }

        public static XElement writeEmptyElement(this XElement writer, string prefix, string namespaceURI, string localName)
        {
            return writer.writeStartElement(prefix, localName, namespaceURI);
        }

        public static XElement writeEmptyElement(this XElement writer, string localName)
        {
            return writer.writeStartElement(localName, "");
        }

        public static XElement writeEmptyElement(this XElement writer, string namespaceURI, string localName)
        {
            return writer.writeStartElement(localName, namespaceURI);
        }

        public static XElement writeStartElement(this XElement parent, string localName, string namespaceURI = "")
        {
            var xname = XName.Get($"{localName}", namespaceURI);

            return new XElement(xname);
        }

        public static XElement writeStartElement(this XElement parent, string prefix, string localName, string namespaceURI = "")
        {
            var xname = XName.Get($"{prefix}:{localName}", namespaceURI);

            return new XElement(xname);
        }

        public static XAttribute writeAttribute(this XElement element, string namespaceUri, string name, object value)
        {
            var attr = new XAttribute(XName.Get($"{name}", namespaceUri), value);

            element.Add(attr);

            return attr;
        }

        public static XAttribute writeAttribute(this XElement element, string prefix, string namespaceUri, string name, object value)
        {
            var attr = new XAttribute(XName.Get($"{prefix}{(string.IsNullOrWhiteSpace(prefix) ? "" : ":")}{name}", namespaceUri), value);

            element.Add(attr);

            return attr;
        }

        public static XAttribute writeAttribute(this XElement element, string name, object value)
        {
            var attr = new XAttribute(name, value);

            element.Add(attr);

            return attr;
        }

        public static XCData writeCharacters(this XElement parent, string data)
        {
            return parent.writeCData(data);
        }

        public static XCData writeCData(this XElement parent, string data)
        {
            var cdata = new XCData(data);

            parent.Add(cdata);

            return cdata;
        }

        public static XElement writeProcessingInstruction(this XElement elem, string target)
        {
            return elem.writeProcessingInstruction(target, "");
        }

        public static XElement writeProcessingInstruction(this XElement elem, string target, string data)
        {
            XProcessingInstruction xpi = new XProcessingInstruction(target, data);

            elem.Add(xpi);

            return elem;
        }

        public static XElement writeDTD(this XElement elem, string dtd)
        {
            elem.CreateWriter().WriteDocType(dtd, "", "", "");

            return elem;
        }

        public static XElement writeEntityRef(this XElement elem, string entityRef)
        {
            XEntity entity = new XEntity(entityRef);

            elem.Add(entity);

            return elem;
        }

        public static XElement writeStartDocument(this XElement elem)
        {
            return elem.writeStartDocument("");
        }

        public static XElement writeStartDocument(this XElement elem, string version)
        {
            return elem.writeStartDocument("utf-8", "");
        }

        public static XElement writeStartDocument(this XElement elem, string encoding, string version)
        {
            var writer = elem.CreateWriter();

            writer.Settings.Encoding = Encoding.GetEncoding(encoding) ?? Encoding.UTF8;

            writer.WriteStartDocument(true);

            return elem;
        }

        public static XElement writeCharacters(this XElement elem, char[] text, int start, int len)
        {
            var writer = elem.CreateWriter();

            writer.WriteChars(text, start, len);

            return elem;
        }

        public static string getPrefix(this XElement elem, string uri)
        {
            return elem.GetPrefixOfNamespace(uri);
        }

        public static XElement setPrefix(this XElement elem, string prefix, string uri)
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
