using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sys.Workflow.Bpmn.Constants;
using Sys.Workflow;

namespace Sys.Workflow.Bpmn.Converters
{
    public class XMLStreamWriter
    {
        private readonly XmlWriter writer;

        public XMLStreamWriter(XmlWriter writer)
        {
            this.writer = writer;
        }

        public XmlWriter XmlWriter => writer;

        public virtual WriteState WriteState => writer.WriteState;

        public string DefaultNamespace { get; set; }

        public virtual void Close()
        {
            writer.Close();
        }

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

        public virtual void WriteCharEntity(char ch)
        {
            writer.WriteCharEntity(ch);
        }

        public virtual void WriteChars(char[] buffer, int index, int count)
        {
            writer.WriteChars(buffer, index, count);
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

        public virtual void WriteCharacters(char[] text, int start, int len)
        {
            writer.WriteChars(text, start, len);
        }

        public virtual void WriteStartDocument(string version)
        {
            writer.WriteStartDocument(true);
        }

        public virtual void WriteStartDocument()
        {
            writer.WriteStartDocument(true);
        }

        public virtual void WriteDTD(string dtd)
        {
            writer.WriteDocType(dtd, "", "", "");
        }

        public virtual void WriteProcessingInstruction(string target, string data)
        {
            writer.WriteProcessingInstruction(target, data);
        }

        public virtual void WriteProcessingInstruction(string target)
        {
            writer.WriteProcessingInstruction(target, "");
        }

        public virtual void WriteComment(string data)
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

        public virtual void WriteStartDocument(string encoding, string v)
        {
            writer.WriteStartDocument(true);
        }

        public virtual void WriteAttribute(string namespaceUri, string name, string value)
        {
            writer.WriteAttributeString(name, namespaceUri, value);
        }

        public virtual void WriteDefaultNamespace(string namespaceUri)
        {
            WriteNamespace(BpmnXMLConstants.BPMN_PREFIX, namespaceUri);
        }

        public virtual void WriteAttribute(string prefix, string namespaceUri, string name, object value)
        {
            writer.WriteAttributeString(prefix, name, namespaceUri, value?.ToString());
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

        public virtual void WriteStartElement(string localName)
        {
            writer.WriteStartElement(localName);
        }

        public virtual void WriteAttribute(string localName, object value)
        {
            writer.WriteAttributeString(localName, value?.ToString());
        }

        public virtual void WriteEndElement()
        {
            writer.WriteEndElement();
        }

        public virtual void WriteCharacters(string documentation)
        {
            if (string.IsNullOrWhiteSpace(documentation))
                return;

            char[] data = Encoding.UTF8.GetChars(Encoding.UTF8.GetBytes(documentation));

            writer.WriteChars(data, 0, data.Length);
        }

        public virtual void WriteCData(string completionCondition)
        {
            writer.WriteCData(completionCondition);
        }

        public virtual void WriteNamespace(string prefix, string namespaceUri)
        {
            writer.WriteAttributeString("xmlns", prefix, null, namespaceUri);
        }

        public virtual void WriteStartElement(string namespaceUri, string localName)
        {
            writer.WriteStartElement(localName, namespaceUri);
        }

        public virtual void WriteEmptyElement(string namespaceURI, string localName)
        {
            writer.WriteStartElement(localName, namespaceURI);
            writer.WriteEndElement();
            //element.WriteEmptyElement(namespaceURI, localName);
        }

        public virtual void WriteEmptyElement(string prefix, string localName, string namespaceURI)
        {
            writer.WriteStartElement(prefix, localName, namespaceURI);
            writer.WriteEndElement();
            //element.WriteEmptyElement(prefix, localName, namespaceURI);
        }

        public virtual void WriteEmptyElement(string localName)
        {
            //element.WriteEmptyElement(localName);
            writer.WriteStartElement(localName);
            writer.WriteEndElement();
        }
    }
}