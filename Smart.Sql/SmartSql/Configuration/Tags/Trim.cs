using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSql.Abstractions;

namespace SmartSql.Configuration.Tags
{
    public class TrimTag : Tag
    {
        public override TagType Type => TagType.Trim;
        public string Prefix { get; set; }
        public string PrefixOverrides { get; set; }
        public string Suffix { get; set; }

        public override bool IsCondition(RequestContext context)
        {
            return true;
        }
    }
}
