using SmartSql.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSql.Configuration.Tags
{
    /// <summary>
    /// 
    /// </summary>
    public class IsFalse : Tag
    {
        public override TagType Type => TagType.IsFalse;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool IsCondition(RequestContext context)
        {
            object reqVal = GetPropertyValue(context);
            if (reqVal is bool req)
            {
                return req == false;
            }
            return false;
        }
    }
}
