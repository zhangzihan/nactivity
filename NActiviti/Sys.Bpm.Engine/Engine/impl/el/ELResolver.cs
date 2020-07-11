using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Workflow.Engine.Impl.EL
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ELResolver
    {
        /// <summary>
        /// 
        /// </summary>
        public static string RESOLVABLE_AT_DESIGN_TIME = "resolvableAtDesignTime";
        /// <summary>
        /// 
        /// </summary>
        public static string TYPE = "type";
        /// <summary>
        /// 
        /// </summary>
        public ELResolver()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="var1"></param>
        /// <param name="var2"></param>
        /// <returns></returns>
        public abstract Type GetCommonPropertyType(ELContext var1, object var2);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="var1"></param>
        /// <param name="base"></param>
        /// <param name="var3"></param>
        /// <returns></returns>
        public abstract Type GetType(ELContext var1, object @base, object var3);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="var1"></param>
        /// <param name="base"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public abstract object GetValue(ELContext var1, object @base, object property);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="var1"></param>
        /// <param name="base"></param>
        /// <param name="var3"></param>
        /// <returns></returns>
        public abstract bool IsReadOnly(ELContext var1, object @base, object var3);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="var1"></param>
        /// <param name="var2"></param>
        /// <param name="var3"></param>
        /// <param name="var4"></param>
        public abstract void SetValue(ELContext var1, object var2, object var3, object var4);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="base"></param>
        /// <param name="method"></param>
        /// <param name="paramTypes"></param>
        /// <param name="params"></param>
        /// <returns></returns>
        public virtual object Invoke(ELContext context, object @base, object method, Type[] paramTypes, object[] @params)
        {
            return null;
        }
    }
}
