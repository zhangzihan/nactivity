using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl.el
{

    public class CompositeELResolver : ELResolver
    {
        private IList<ELResolver> resolvers = new List<ELResolver>();

        public CompositeELResolver()
        {
        }

        public IList<ELResolver> Resolvers => resolvers;

        public void add(ELResolver elResolver)
        {
            if (elResolver == null)
            {
                throw new ArgumentNullException("resolver must not be null");
            }
            else
            {
                this.resolvers.Add(elResolver);
            }
        }

        public override Type getCommonPropertyType(ELContext context, object @base)
        {
            Type result = null;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                Type type = ((ELResolver)this.resolvers[i]).getCommonPropertyType(context, @base);
                if (type != null)
                {
                    if (result != null && !type.IsAssignableFrom(result))
                    {
                        if (!result.IsAssignableFrom(type))
                        {
                            result = type;
                        }
                    }
                    else
                    {
                        result = type;
                    }
                }
            }

            return result;
        }

        public override Type getType(ELContext context, object @base, object property)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                Type type = ((ELResolver)this.resolvers[i]).getType(context, @base, property);
                if (context.IsPropertyResolved)
                {
                    return type;
                }
            }

            return null;
        }

        public override object getValue(ELContext context, object @base, object property)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                Object value = ((ELResolver)this.resolvers[i]).getValue(context, @base, property);
                if (context.IsPropertyResolved)
                {
                    return value;
                }
            }

            return null;
        }

        public override bool isReadOnly(ELContext context, object @base, object property)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                bool readOnly = ((ELResolver)this.resolvers[i]).isReadOnly(context, @base, property);
                if (context.IsPropertyResolved)
                {
                    return readOnly;
                }
            }

            return false;
        }

        public override void setValue(ELContext context, object @base, object property, object value)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                ((ELResolver)this.resolvers[i]).setValue(context, @base, property, value);
                if (context.IsPropertyResolved)
                {
                    return;
                }
            }

        }

        public override object invoke(ELContext context, object @base, object method, Type[] paramTypes, object[] @params)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                object result = ((ELResolver)this.resolvers[i]).invoke(context, @base, method, paramTypes, @params);
                if (context.IsPropertyResolved)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
