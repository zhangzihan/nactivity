using org.activiti.bpmn.constants;
using org.activiti.engine.impl.bpmn.behavior;
using System.Collections.Generic;
using System.Linq;

namespace org.activiti.engine.impl.persistence.entity
{
    public class AssigneeTypes : Dictionary<string, string>
    {
        public AssigneeTypes()
        {
            this[AssigneeType.ONE] = $"{typeof(OnePassParallelMultiInstanceBehavior).FullName},{typeof(OnePassParallelMultiInstanceBehavior).Assembly.GetName().Name}";
            this[AssigneeType.ALL] = $"{typeof(AllPassParallelMultiInstanceBehavior).FullName},{typeof(AllPassParallelMultiInstanceBehavior).Assembly.GetName().Name}";
            this[AssigneeType.SINGLE] = $"{typeof(ParallelMultiInstanceBehavior).FullName},{typeof(ParallelMultiInstanceBehavior).Assembly.GetName().Name}";
        }

        public bool ContainsType(string typeName)
        {
            string type = typeName.Split(new char[] { ',' })[0];

            return this.Values.Any(x => string.Compare(x.Split(new char[] { ',' })[0], type, true) == 0);
        }
    }
}
