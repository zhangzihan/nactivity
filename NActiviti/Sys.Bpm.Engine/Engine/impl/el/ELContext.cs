using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Workflow.engine.impl.el
{
    public abstract class ELContext
    {
        private System.Collections.Generic.Dictionary<Type, object> context;
        //private Locale locale;
        private bool resolved;

        public ELContext()
        {
        }

        public Object GetContext(Type key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key is null");
            }
            else
            {
                return context?[key];
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

        public void PutContext(Type key, Object contextObject)
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
