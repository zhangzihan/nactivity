using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.activiti.engine.impl.el
{

    public abstract class ELResolver
    {
        public static string RESOLVABLE_AT_DESIGN_TIME = "resolvableAtDesignTime";
        public static string TYPE = "type";

        public ELResolver()
        {
        }

        public abstract Type getCommonPropertyType(ELContext var1, object var2);

        public abstract Type getType(ELContext var1, object @base, object var3);

        public abstract object getValue(ELContext var1, object @base, object property);

        public abstract bool isReadOnly(ELContext var1, object @base, object var3);

        public abstract void setValue(ELContext var1, object var2, object var3, object var4);

        public virtual object invoke(ELContext context, object @base, object method, Type[] paramTypes, object[] @params)
        {
            return null;
        }
    }
}
