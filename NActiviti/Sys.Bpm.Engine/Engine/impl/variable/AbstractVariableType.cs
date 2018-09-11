using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.activiti.engine.impl.variable
{
    public abstract class AbstractVariableType : IVariableType
    {
        public abstract string TypeName { get; }
        public abstract bool Cachable { get; }

        public abstract object getValue(IValueFields valueFields);
        public abstract bool isAbleToStore(object value);
        public abstract void setValue(object value, IValueFields valueFields);

        public override string ToString()
        {
            var type = this.GetType();

            //return $"{type.FullName},{type.Assembly.GetName().Name}";
            return TypeName;
        }
    }
}
