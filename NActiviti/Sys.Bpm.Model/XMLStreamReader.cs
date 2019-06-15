using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace org.activiti.bpmn.converter
{
    public class XMLStreamReader : XmlReader
    {
        protected XmlReader reader = null;
        protected internal XElement element = null;
        private readonly XDocument document = null;
        internal bool isEmpty;

        public XMLStreamReader(Stream input)
        {
            document = XDocument.Load(input, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
            element = document.Root;
            input.Seek(0, SeekOrigin.Begin);
            this.reader = element.CreateReader(ReaderOptions.OmitDuplicateNamespaces);
        }

        public override int AttributeCount => reader.AttributeCount;

        public override string BaseURI => reader.BaseURI;

        public override int Depth
        {
            get
            {
                return reader.Depth;
            }
        }

        public override bool EOF => reader.EOF;

        public override bool IsEmptyElement => reader.IsEmptyElement;

        public override string LocalName => reader.LocalName;

        public override string NamespaceURI => reader.NamespaceURI;

        public override XmlNameTable NameTable => reader.NameTable;

        public override XmlNodeType NodeType => reader.NodeType;

        public override string Prefix => reader.Prefix;

        public override ReadState ReadState => reader.ReadState;

        public bool EndElement => reader.NodeType == XmlNodeType.EndElement || reader.IsEmptyElement;

        public new bool IsStartElement() { return reader.IsStartElement(); }

        public new bool IsStartElement(string name)
        {
            return reader.IsStartElement(name);
        }

        public new bool IsStartElement(string localname, string ns)
        {
            return reader.IsStartElement(localname, ns);
        }

        public string ElementText => element.Value;

        public int NamespaceCount => element.Attributes().Where(x => x.IsNamespaceDeclaration).Count();

        public override string Value => element.Value;

        public override string GetAttribute(int i)
        {
            return reader.GetAttribute(i);
        }

        public override string GetAttribute(string name)
        {
            return reader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return reader.GetAttribute(name, NamespaceURI);
        }

        public override string LookupNamespace(string prefix)
        {
            return reader.LookupNamespace(prefix);
        }

        public override bool MoveToAttribute(string name)
        {
            return reader.MoveToAttribute(name);
        }

        public string GetNamespacePrefix(int i)
        {
            var attr = element.Attributes().Where(x => x.IsNamespaceDeclaration).ElementAt(i);

            return attr.Name.LocalName;
        }

        public string GetNamespaceURI(int i)
        {
            var attr = element.Attributes().Where(x => x.IsNamespaceDeclaration).ElementAt(i);

            return attr?.Value;
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return reader.MoveToAttribute(name, ns);
        }

        public override bool MoveToElement()
        {
            return reader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return reader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return reader.MoveToNextAttribute();
        }

        public override bool Read()
        {
            return reader.Read();
        }

        public override bool ReadAttributeValue()
        {
            return reader.ReadAttributeValue();
        }

        public override void ResolveEntity()
        {
            reader.ResolveEntity();
        }

        public IXmlLineInfo LineInfo => reader as IXmlLineInfo;

        public bool HasNext()
        {
            if (isEmpty)
            {
                isEmpty = false;
                return true;
            }

            bool next = reader.Read();

            isEmpty = IsEmptyElement;

            if (next && reader.IsStartElement())
            {
                string name = reader.LocalName;
                element = document.Descendants(XName.Get(name, reader.NamespaceURI)).FirstOrDefault(x =>
                {
                    var eline = x as IXmlLineInfo;
                    return LineInfo.LineNumber == eline.LineNumber && LineInfo.LinePosition == eline.LinePosition;
                });
            }

            return next;
        }

        public bool Next()
        {
            return HasNext();
        }

        public string GetAttributeValue(string name)
        {
            return reader.GetAttribute(name);
        }

        public string GetAttributeValue(string namespaceUri, string name)
        {
            return reader.GetAttribute(name, namespaceUri);
        }
    }
}