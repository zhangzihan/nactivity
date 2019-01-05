using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.activiti.engine.impl.el
{
    public abstract class ELContext
    {
        private System.Collections.Generic.Dictionary<Type, object> context;
        //private Locale locale;
        private bool resolved;

        public ELContext()
        {
        }

        public Object getContext(Type key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key is null");
            }
            else
            {
                return this.context == null ? null : this.context[key];
            }
        }

        public abstract ELResolver ELResolver { get; }

        public abstract FunctionMapper FunctionMapper { get; }


        public abstract VariableMapper VariableMapper { get; }

        public bool IsPropertyResolved
        {
            get
            {
                return this.resolved;
            }
            set
            {
                this.resolved = value;
            }
        }

        public void putContext(Type key, Object contextObject)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key is null");
            }
            else
            {
                if (this.context == null)
                {
                    this.context = new Dictionary<Type, object>();
                }

                this.context.Add(key, contextObject);
            }
        }
    }

    public class FunctionMapper
    { }

    public class VariableMapper
    {

    }

}
