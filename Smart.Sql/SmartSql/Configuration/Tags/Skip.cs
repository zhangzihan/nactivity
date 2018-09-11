using System.Xml;
using SmartSql.Abstractions;

namespace SmartSql.Configuration.Tags
{
    class SkipTag : ITag
    {
        public TagType Type => TagType.Skip;

        public ITag Parent { get; set; }
        public XmlLineInfo LineInfo
        {
            get; set;
        }

        public void BuildSql(RequestContext context)
        {
            throw new System.NotImplementedException();
        }

        public bool IsCondition(RequestContext context)
        {
            return true;
        }
    }
}