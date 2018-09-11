using System;
using System.Linq;
using SmartSql.Abstractions;
using SmartSql.Configuration.Statements;
using Sys.Expressions;

namespace SmartSql.Configuration.Tags
{
    public class Switch : Tag
    {
        public override TagType Type => TagType.Switch;

        public override bool IsCondition(RequestContext context)
        {
            return true;
        }
        public override void BuildSql(RequestContext context)
        {
            var matchedTag = ChildTags.FirstOrDefault(tag =>
            {
                if (tag.Type == TagType.SwitchCase)
                {
                    var caseTag = tag as Case;
                    return caseTag.IsCondition(context);
                }
                return false;
            });
            if (matchedTag == null)
            {
                matchedTag = ChildTags.FirstOrDefault(tag => tag.Type == TagType.SwitchDefault);
            }
            if (matchedTag != null)
            {
                matchedTag.BuildSql(context);
            }

        }

        public class Case : CompareTag
        {
            public override TagType Type => TagType.SwitchCase;
            public string Test { get; set; }
            public override bool IsCondition(RequestContext context)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(Test))
                    {
                        var obj = ExpressionManager.GetValue(context.Request, Test, context.RequestParameters);

                        if (bool.TryParse(obj?.ToString(), out var ret) == false)
                        {
                            return false;
                        }

                        return ret;
                    }
                    else
                    {
                        var reqVal = GetPropertyValue(context);
                        if (reqVal == null) { return false; }
                        string reqValStr = string.Empty;
                        if (reqVal is Enum)
                        {
                            reqValStr = reqVal.GetHashCode().ToString();
                        }
                        else
                        {
                            reqValStr = reqVal.ToString();
                        }
                        return reqValStr.Equals(CompareValue);
                    }
                }
                catch (Exception e)
                {
                    throw new BuildSqlException($"构造SQL语句发生错误({LineInfo.LineNumber},{LineInfo.LinePosition}):{e.Message}", e);
                }
            }
        }

        public class Defalut : Tag
        {
            public override TagType Type => TagType.SwitchDefault;

            public override bool IsCondition(RequestContext context)
            {
                return true;
            }
        }
    }
}
