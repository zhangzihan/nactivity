using Sys.Workflow.Bpmn.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Sys.Workflow.Bpmn.Converters
{
	public class XMLStreamReader : XmlReader
	{
		protected XmlReader reader = null;
		protected internal XElement element = null;
		private XDocument document = null;
		internal bool isEmpty;

		class EdgeXElementCompare : EqualityComparer<XElement>
		{
			public override bool Equals(XElement x, XElement y)
			{
				if (x is null && y is null)
				{
					return true;
				}

				if ((x is not null && y is null) || (x is null) && (y is not null))
				{
					return false;
				}

				var xx = x.Attribute("x")?.Value;
				var xy = x.Attribute("y")?.Value;

				var yx = y.Attribute("x")?.Value;
				var yy = y.Attribute("y")?.Value;

				return true == xx?.Equals(yx) && true == xy?.Equals(yy);
			}

			public override int GetHashCode([DisallowNull] XElement obj)
			{
				var x = obj.Attribute("x")?.Value;
				var y = obj.Attribute("y")?.Value;

				return HashCode.Combine(x?.GetHashCode() ?? 0, y?.GetHashCode() ?? 0);
			}
		}

		private static readonly EdgeXElementCompare exc = new EdgeXElementCompare();

		public XMLStreamReader(Stream input)
		{
			if (input is null)
			{
				throw new ArgumentNullException(nameof(input));
			}
			CreateXMLStreamReader(input);
		}

		public XMLStreamReader(string bpmnXml)
		{
			if (string.IsNullOrWhiteSpace(bpmnXml))
			{
				throw new ArgumentException($"“{nameof(bpmnXml)}”不能为 null 或空白。", nameof(bpmnXml));
			}

			using var ms = new MemoryStream(Encoding.UTF8.GetBytes(bpmnXml));
			CreateXMLStreamReader(ms);
		}

		private void CreateXMLStreamReader(Stream input)
		{
			document = XDocument.Load(input, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
			using var ms = new MemoryStream();
			input.CopyTo(ms);
			var edges = document.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_DI_EDGE, BpmnXMLConstants.BPMNDI_NAMESPACE));
			foreach (var edge in edges)
			{
				var elements = edge.Elements();
				var dises = elements.Distinct(exc).ToArray();
				if (dises.Length != elements.Count())
				{
					edge.RemoveNodes();
					edge.Add(dises);
				}
			}
			ms.SetLength(0);
			document.Save(ms);
			ms.Flush();
			ms.Seek(0, SeekOrigin.Begin);
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

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (reader is not null)
			{
				reader.Close();
			}
		}
	}
}