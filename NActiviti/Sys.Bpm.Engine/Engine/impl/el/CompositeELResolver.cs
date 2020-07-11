using System;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.EL
{
    /// <summary>
    /// 
    /// </summary>
    public class CompositeELResolver : ELResolver
    {
        private readonly IList<ELResolver> resolvers = new List<ELResolver>();

        /// <summary>
        /// 
        /// </summary>
        public CompositeELResolver()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<ELResolver> Resolvers => resolvers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elResolver"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="base"></param>
        /// <returns></returns>
        public override Type GetCommonPropertyType(ELContext context, object @base)
        {
            Type result = null;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                Type type = this.resolvers[i].GetCommonPropertyType(context, @base);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="base"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override Type GetType(ELContext context, object @base, object property)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                Type type = this.resolvers[i].GetType(context, @base, property);
                if (context.IsPropertyResolved)
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="base"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override object GetValue(ELContext context, object @base, object property)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                object value = this.resolvers[i].GetValue(context, @base, property);
                if (context.IsPropertyResolved)
                {
                    return value;
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="base"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override bool IsReadOnly(ELContext context, object @base, object property)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                bool readOnly = this.resolvers[i].IsReadOnly(context, @base, property);
                if (context.IsPropertyResolved)
                {
                    return readOnly;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="base"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public override void SetValue(ELContext context, object @base, object property, object value)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                this.resolvers[i].SetValue(context, @base, property, value);
                if (context.IsPropertyResolved)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="base"></param>
        /// <param name="method"></param>
        /// <param name="paramTypes"></param>
        /// <param name="params"></param>
        /// <returns></returns>
        public override object Invoke(ELContext context, object @base, object method, Type[] paramTypes, object[] @params)
        {
            context.IsPropertyResolved = false;
            int i = 0;

            for (int l = this.resolvers.Count; i < l; ++i)
            {
                object result = this.resolvers[i].Invoke(context, @base, method, paramTypes, @params);
                if (context.IsPropertyResolved)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
