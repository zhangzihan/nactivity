using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.activiti.engine.impl.persistence
{
    public class PersistentState : Dictionary<string, object>
    {
        public PersistentState() : base()
        {
        }

        public PersistentState Clone()
        {
            var clone = new PersistentState();

            foreach (string key in this.Keys)
            {
                clone.Add(key, this[key]);
            }

            return clone;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (obj is PersistentState == false)
            {
                return false;
            }

            PersistentState other = obj as PersistentState;
            if (this.Keys.Count != other.Keys.Count)
            {
                return false;
            }

            var equal = JsonConvert.SerializeObject(this, Formatting.None) == JsonConvert.SerializeObject(other, Formatting.None);

            return equal;

            //foreach (string key in this.Keys)
            //{
            //    if (other.TryGetValue(key, out var ov))
            //    {
            //        if (ov != this[key])
            //        {
            //            return false;
            //        }
            //    }
            //}

            //return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
