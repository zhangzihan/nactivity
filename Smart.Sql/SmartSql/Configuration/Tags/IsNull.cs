using SmartSql.Abstractions;
using Sys.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSql.Configuration.Tags
{
    public class IsNull : Tag
    {
        public override TagType Type => TagType.IsNull;

        public override bool IsCondition(RequestContext context)
        {
            Object reqVal = GetPropertyValue(context);
            return reqVal is null;
        }
    }
}
