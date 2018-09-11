using SmartSql.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SmartSql.Configuration.Tags
{
    public class SqlText : ITag
    {
        public TagType Type => TagType.SqlText;
        public string BodyText { get; set; }
        public ITag Parent { get; set; }
        public XmlLineInfo LineInfo { get; set; }

        public void BuildSql(RequestContext context)
        {
            if (context.IsFirstDyChild)
            {
                if (Parent != null)
                {
                    var dyParent = Parent as Dynamic;
                    context.Sql.Append($" {dyParent.Prepend} ");
                }
                context.IsFirstDyChild = false;
            }
            context.Sql.Append(BodyText);
        }

        public bool IsCondition(RequestContext context)
        {
            return true;
        }

    }
}
