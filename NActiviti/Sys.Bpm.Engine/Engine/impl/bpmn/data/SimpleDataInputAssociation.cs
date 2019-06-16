using System;
using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sys.Workflow.engine.impl.bpmn.data
{

    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// <summary>
    /// A simple data input association between a source and a target with assignments
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class SimpleDataInputAssociation : AbstractDataAssociation
    {

        private const long serialVersionUID = 1L;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<Assignment> assignments = new List<Assignment>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceExpression"></param>
        /// <param name="target"></param>
        public SimpleDataInputAssociation(IExpression sourceExpression, string target) : base(sourceExpression, target)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public SimpleDataInputAssociation(string source, string target) : base(source, target)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assignment"></param>
        public virtual void AddAssignment(Assignment assignment)
        {
            this.assignments.Add(assignment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public override void Evaluate(IExecutionEntity execution)
        {
            foreach (Assignment assignment in this.assignments)
            {
                assignment.Evaluate(execution);
            }
        }
    }
}