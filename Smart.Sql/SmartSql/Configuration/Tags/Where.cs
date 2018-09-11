using System;
using System.Collections.Generic;
using System.Text;
using SmartSql.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartSql.Configuration.Tags
{
    public class Where : Dynamic
    {
        public override TagType Type => TagType.Where;
        public override string Prepend { get { return "Where"; } }

        private static readonly Regex WHERE_PATTERN = new Regex("(where\\s+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex WHERE_TRIM = new Regex("where\\s+(and|or)\\b", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override void BuildSql(RequestContext context)
        {
            base.BuildSql(context);

            var sql = context.Sql.ToString();

            if (WHERE_PATTERN.IsMatch(sql))
            {
                sql = WHERE_PATTERN.Replace(sql, " ");
                context.Sql = new StringBuilder(sql);
            }

            if (WHERE_TRIM.IsMatch(sql))
            {
                sql = WHERE_TRIM.Replace(sql, (m) =>
                {
                    return "where ";
                });
                context.Sql = new StringBuilder(sql);
            }
        }

        public override void BuildChildSql(RequestContext context)
        {
            base.BuildChildSql(context);
        }
    }
}
