using SmartSql.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SmartSql.Configuration.Tags
{
    public class XmlLineInfo
    {
        private XmlLineInfo()
        {

        }

        public int LineNumber { get; private set; }

        public int LinePosition { get; private set; }

        public bool HasLineInfo { get; private set; }

        public static XmlLineInfo Create(IXmlLineInfo node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            return new XmlLineInfo
            {
                LineNumber = node.LineNumber,
                LinePosition = node.LinePosition,
                HasLineInfo = node.HasLineInfo()
            };
        }
    }

    public interface ITag
    {
        TagType Type { get; }
        ITag Parent { get; set; }
        XmlLineInfo LineInfo { get; set; }
        bool IsCondition(RequestContext context);
        void BuildSql(RequestContext context);
    }

    public enum TagType
    {
        SqlText,
        IsEmpty,
        IsEqual,
        IsGreaterEqual,
        IsGreaterThan,
        IsLessEqual,
        IsLessThan,
        IsNotEmpty,
        IsNotEqual,
        IsNotNull,
        IsNull,
        IsTrue,
        IsFalse,
        IsProperty,
        Placeholder,
        Include,
        Switch,
        SwitchCase,
        SwitchDefault,
        Dynamic,
        Where,
        Set,
        If,
        For,
        Env,
        Skip,
        Take,
        OrderBy,
        GroupBy,
        Bind,
        Trim
    }
}
