using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSql.Abstractions;

namespace SmartSql.Configuration.Tags
{
    public class BindTag : Tag
    {
        public override TagType Type => TagType.Bind;

        public string Name { get; set; }

        public string Value { get; set; }

        public override bool IsCondition(RequestContext context)
        {
            return true;
        }
    }
}
