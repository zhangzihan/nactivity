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
    public abstract class ELContext
    {
        private Dictionary<Type, object> context;
        //private Locale locale;
        private bool resolved;
        /// <summary>
        /// 
        /// </summary>
        public ELContext()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetContext(Type key)
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
        /// <summary>
        /// 
        /// </summary>
        public abstract ELResolver ELResolver { get; }
        /// <summary>
        /// 
        /// </summary>
        public abstract FunctionMapper FunctionMapper { get; }

        /// <summary>
        /// 
        /// </summary>
        public abstract VariableMapper VariableMapper { get; }
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="contextObject"></param>
        public void PutContext(Type key, object contextObject)
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
    /// <summary>
    /// 
    /// </summary>
    public class FunctionMapper
    { }
    /// <summary>
    /// 
    /// </summary>
    public class VariableMapper
    {

    }

}
