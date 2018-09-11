using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Bpm.Engine
{
    public class KeyValueParameter<TKey, TValue>
    {
        private KeyValuePair<TKey, TValue> value;

        public KeyValueParameter(TKey key, TValue value)
        {
            this.value = new KeyValuePair<TKey, TValue>(key, value);
        }

        public TKey Key { get => this.value.Key; }

        public TValue Value { get => this.value.Value; }

        public static implicit operator string(KeyValueParameter<TKey, TValue> parameter)
        {
            if (parameter == null || !(parameter is KeyValueParameter<TKey, TValue>))
            {
                return null;
            }

            return parameter.Value?.ToString();
        }

        public override string ToString()
        {
            return this.value.ToString();
        }
    }
}
