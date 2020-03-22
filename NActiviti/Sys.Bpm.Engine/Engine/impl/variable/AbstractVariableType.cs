using Sys.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Workflow.Engine.Impl.Variable
{
    public abstract class AbstractVariableType : IVariableType
    {
        public abstract string TypeName { get; }
        public abstract bool Cachable { get; }

        public ISerializableTypeSerializer Serializer
        {
            get
            {
                var serializer = ProcessEngineServiceProvider.Resolve<ISerializableTypeSerializer>();
                if (serializer == null)
                {
                    serializer = new DefaultSerializableTypeSerializer();
                }

                return serializer;
            }
        }

        public abstract object GetValue(IValueFields valueFields);
        public abstract bool IsAbleToStore(object value);
        public abstract void SetValue(object value, IValueFields valueFields);

        public override string ToString()
        {
            var type = this.GetType();

            //return $"{type.FullName},{type.Assembly.GetName().Name}";
            return TypeName;
        }
    }
}
