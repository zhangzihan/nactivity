using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Workflow.Engine.Impl.EL
{

    public abstract class ELResolver
    {
        public static string RESOLVABLE_AT_DESIGN_TIME = "resolvableAtDesignTime";
        public static string TYPE = "type";

        public ELResolver()
        {
        }

        public abstract Type GetCommonPropertyType(ELContext var1, object var2);

        public abstract Type GetType(ELContext var1, object @base, object var3);

        public abstract object GetValue(ELContext var1, object @base, object property);

        public abstract bool IsReadOnly(ELContext var1, object @base, object var3);

        public abstract void SetValue(ELContext var1, object var2, object var3, object var4);

        public virtual object Invoke(ELContext context, object @base, object method, Type[] paramTypes, object[] @params)
        {
            return null;
        }
    }
}
