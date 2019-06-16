using System;
using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.el
{

    public class CompositeELResolver : ELResolver
    {
        private readonly IList<ELResolver> resolvers = new List<ELResolver>();

        public CompositeELResolver()
        {
        }

        public IList<ELResolver> Resolvers => resolvers;

        public void Add(ELResolver elResolver)
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

        public override Type GetCommonPropertyType(ELContext context, object @base)
        {
            Type result = null;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                Type type = ((ELResolver)this.resolvers[i]).GetCommonPropertyType(context, @base);
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

        public override Type GetType(ELContext context, object @base, object property)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                Type type = ((ELResolver)this.resolvers[i]).GetType(context, @base, property);
                if (context.IsPropertyResolved)
                {
                    return type;
                }
            }

            return null;
        }

        public override object GetValue(ELContext context, object @base, object property)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                Object value = ((ELResolver)this.resolvers[i]).GetValue(context, @base, property);
                if (context.IsPropertyResolved)
                {
                    return value;
                }
            }

            return null;
        }

        public override bool IsReadOnly(ELContext context, object @base, object property)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                bool readOnly = ((ELResolver)this.resolvers[i]).IsReadOnly(context, @base, property);
                if (context.IsPropertyResolved)
                {
                    return readOnly;
                }
            }

            return false;
        }

        public override void SetValue(ELContext context, object @base, object property, object value)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                ((ELResolver)this.resolvers[i]).SetValue(context, @base, property, value);
                if (context.IsPropertyResolved)
                {
                    return;
                }
            }

        }

        public override object Invoke(ELContext context, object @base, object method, Type[] paramTypes, object[] @params)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                object result = ((ELResolver)this.resolvers[i]).Invoke(context, @base, method, paramTypes, @params);
                if (context.IsPropertyResolved)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
