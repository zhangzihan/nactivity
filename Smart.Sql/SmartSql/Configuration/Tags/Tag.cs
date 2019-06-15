using Newtonsoft.Json.Linq;
using SmartSql.Abstractions;
using SmartSql.Configuration.Statements;
using Sys.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SmartSql.Configuration.Tags
{
    public abstract class Tag : ITag
    {
        private IList<ITag> _childTags;

        public virtual String Prepend { get; set; }
        public String Property { get; set; }
        public abstract TagType Type { get; }
        public IList<ITag> ChildTags
        {
            get
            {
                if (_childTags == null)
                {
                    _childTags = new List<ITag>();
                }
                return _childTags;
            }
            set
            {
                _childTags = value;
            }
        }

        public XmlLineInfo LineInfo { get; set; }

        public ITag Parent { get; set; }
        public abstract bool IsCondition(RequestContext context);
        public virtual void BuildSql(RequestContext context)
        {
            try
            {
                if (IsCondition(context))
                {
                    context.Sql.Append(" ");
                    if (context.IsFirstDyChild)
                    {
                        if (Parent is Dynamic dyParent)
                        {
                            context.Sql.Append(dyParent.Prepend);
                        }
                        context.IsFirstDyChild = false;
                    }
                    else
                    {
                        context.Sql.Append(Prepend);
                    }
                    context.Sql.Append(" ");
                    BuildChildSql(context);
                }
            }
            catch (Exception e)
            {
                throw new BuildSqlException($"构造SQL语句发生错误({LineInfo.LineNumber},{LineInfo.LinePosition}):{e.Message}", e);
            }
        }

        public virtual void BuildChildSql(RequestContext context)
        {
            if (ChildTags != null && ChildTags.Count > 0)
            {
                foreach (var childTag in ChildTags)
                {
                    childTag.BuildSql(context);
                }
            }
        }
        protected virtual String GetDbProviderPrefix(RequestContext context)
        {
            return context.SmartSqlContext.DbPrefix;
        }

        protected virtual Object GetPropertyValue(RequestContext context)
        {
            var reqParams = context.RequestParameters;
            if (reqParams == null)
            {
                return null;
            }

            if (Property == null)
            {
                throw new PropertyArgumentNullException($"Tag property null {ToString()}");
            }

            if (!string.IsNullOrWhiteSpace(Property) && context.Request != null)
            {
                return ExpressionManager.GetValue(context.Request, Property, context.RequestParameters);
            }

            reqParams.TryGetValue(Property, out object propVal);

            return propVal;
        }
    }
}
