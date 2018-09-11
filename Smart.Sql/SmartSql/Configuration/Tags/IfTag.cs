using SmartSql.Abstractions;
using Sys.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSql.Configuration.Tags
{
    public class IfTag : Tag
    {
        private string _test;

        public override TagType Type => TagType.If;

        public string Test { get => _test; set => _test = value; }

        public override bool IsCondition(RequestContext context)
        {
            if (string.IsNullOrWhiteSpace(Test))
            {
                return true;
            }

            var res = ExpressionManager.GetValue(context.Request, this.Test, context.RequestParameters);

            if (!bool.TryParse(res?.ToString(), out var ret))
            {
                return false;
            }

            return ret;
        }
    }
}
