using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Workflow.Engine.Impl.Persistence
{
    /// <summary>
    /// 
    /// </summary>
    public class PersistentState : Dictionary<string, object>
    {
        /// <summary>
        /// 
        /// </summary>
        public PersistentState() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public PersistentState Clone()
        {
            var clone = new PersistentState();

            foreach (string key in this.Keys)
            {
                clone.Add(key, this[key]);
            }

            return clone;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
