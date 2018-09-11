using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sys.Bpm;

namespace org.activiti.bpmn.converter
{
    public class XMLStreamWriter : XElement
    {
        protected XElement element;
        protected XmlWriter writer;

        public XMLStreamWriter(XMLStreamWriter writer, XElement element) : this(element)
        {
            this.writer = writer.writer;
        }

        public XMLStreamWriter(XElement element) : base(element)
        {
            this.element = element;
            this.writer = element.CreateWriter();
        }

        public virtual WriteState WriteState => writer.WriteState;

        public string DefaultNamespace { get; set; }

        public virtual void Flush()
        {
            writer.Flush();
        }

        public virtual string LookupPrefix(string ns)
        {
            return writer.LookupPrefix(ns);
        }

        public virtual void WriteBase64(byte[] buffer, int index, int count)
        {
            writer.WriteBase64(buffer, index, count);
        }

        public virtual void WriteCData(string text)
        {
            writer.WriteCData(text);
        }

        public virtual void WriteCharEntity(char ch)
        {
            writer.WriteCharEntity(ch);
        }

        public virtual void WriteChars(char[] buffer, int index, int count)
        {
            writer.WriteChars(buffer, index, count);
        }

        public virtual void WriteComment(string text)
        {
            writer.WriteComment(text);
        }

        public virtual void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            writer.WriteDocType(name, pubid, sysid, subset);
        }

        public virtual void WriteEndAttribute()
        {
            writer.WriteEndAttribute();
        }

        public virtual void WriteEndDocument()
        {
            writer.WriteEndDocument();
        }

        public virtual void writeEndDocument()
        {
            WriteEndDocument();
        }

        public virtual void WriteEndElement()
        {
            writer.WriteEndElement();
        }

        public virtual void setPrefix(string prefix, string uri)
        {
            element.setPrefix(prefix, uri);
        }

        public virtual string getPrefix(string uri)
        {
            return element.GetPrefixOfNamespace(uri);
        }

        public virtual void writeCharacters(char[] text, int start, int len)
        {
            writer.WriteChars(text, start, len);
        }

        public virtual void writeStartDocument(string version)
        {
            writer.WriteStartDocument(true);
        }

        public virtual void writeStartDocument()
        {
            writer.WriteStartDocument(true);
        }

        public virtual void writeEntityRef(string name)
        {
            element.writeEntityRef(name);
        }

        public virtual void writeDTD(string dtd)
        {
            writer.WriteDocType(dtd, "", "", "");
        }

        public virtual void writeProcessingInstruction(string target, string data)
        {
            writer.WriteProcessingInstruction(target, data);
        }

        public virtual void writeProcessingInstruction(string target)
        {
            writer.WriteProcessingInstruction(target, "");
        }

        public virtual void writeComment(string data)
        {
            this.writer.WriteComment(data);
        }

        public virtual void WriteEntityRef(string name)
        {
            writer.WriteEntityRef(name);
        }

        public virtual void WriteFullEndElement()
        {
            writer.WriteFullEndElement();
        }

        public virtual void WriteProcessingInstruction(string name, string text)
        {
            writer.WriteProcessingInstruction(name, text);
        }

        public virtual void WriteRaw(char[] buffer, int index, int count)
        {
            writer.WriteRaw(buffer, index, count);
        }

        public virtual void WriteRaw(string data)
        {
            writer.WriteRaw(data);
        }

        public virtual void WriteStartAttribute(string prefix, string localName, string ns)
        {
            writer.WriteStartAttribute(prefix, localName, ns);
        }

        public virtual void writeStartDocument(string encoding, string v)
        {
            writer.WriteStartDocument(true);
        }

        public virtual void writeAttribute(string namespaceUri, string name, string value)
        {
            writer.WriteAttributeString(name, namespaceUri, value);
        }

        public virtual void writeDefaultNamespace(string namespaceUri)
        {
            writer.WriteAttributeString(constants.BpmnXMLConstants.TARGET_NAMESPACE_ATTRIBUTE, namespaceUri);
        }

        public virtual void writeAttribute(string prefix, string namespaceUri, string name, object value)
        {
            writer.WriteAttributeString(prefix, name, namespaceUri, value?.ToString());
        }

        public virtual void WriteStartDocument()
        {
            writer.WriteStartDocument();
        }

        public virtual void WriteStartDocument(bool standalone)
        {
            writer.WriteStartDocument(standalone);
        }

        public virtual void WriteStartElement(string prefix, string localName, string ns)
        {
            writer.WriteStartElement(prefix, localName, ns);
        }

        public virtual void WriteString(string text)
        {
            writer.WriteString(text);
        }

        public virtual void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            writer.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public virtual void WriteWhitespace(string ws)
        {
            writer.WriteWhitespace(ws);
        }

        public virtual void writeStartElement(string localName)
        {
            writer.WriteStartElement(localName);
        }

        public virtual void writeAttribute(string localName, object value)
        {
            writer.WriteAttributeString(localName, value?.ToString());
        }

        public virtual void writeEndElement()
        {
            writer.WriteEndElement();
        }

        public virtual void writeCharacters(string documentation)
        {
            if (string.IsNullOrWhiteSpace(documentation))
                return;

            char[] data = Encoding.UTF8.GetChars(Encoding.UTF8.GetBytes(documentation));

            writer.WriteChars(data, 0, data.Length);
        }

        public virtual void writeCData(string completionCondition)
        {
            writer.WriteCData(completionCondition);
        }

        public virtual void writeNamespace(string prefix, string namespaceUri)
        {
            element.writeNamespace(prefix, namespaceUri);
        }

        public virtual void writeStartElement(string prefix, string localname, string ns)
        {
            writer.WriteStartElement(prefix, localname, ns);
        }

        public virtual void writeStartElement(string namespaceUri, string localName)
        {
            writer.WriteStartElement(localName, namespaceUri);
        }

        public virtual void writeEmptyElement(string namespaceURI, string localName)
        {
            element.writeEmptyElement(namespaceURI, localName);
        }

        public virtual void writeEmptyElement(string prefix, string localName, string namespaceURI)
        {
            element.writeEmptyElement(prefix, localName, namespaceURI);
        }

        public virtual void writeEmptyElement(string localName)
        {
            element.writeEmptyElement(localName);
        }
    }
}