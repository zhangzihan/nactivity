using System;

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
namespace Sys.Workflow.engine.impl.bpmn.webservice
{
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.bpmn.behavior;
    using Sys.Workflow.engine.impl.bpmn.data;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// <summary>
    /// An implicit data input association between a source and a target. source is a variable in the current execution context and target is a property in the message
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class MessageImplicitDataInputAssociation : AbstractDataAssociation
    {

        private const long serialVersionUID = 1L;

        public MessageImplicitDataInputAssociation(string source, string target) : base(source, target)
        {
        }

        public override void Evaluate(IExecutionEntity execution)
        {
            if (!string.IsNullOrWhiteSpace(this.source))
            {
                object value = execution.GetVariable(this.source);
                MessageInstance message = (MessageInstance)execution.GetVariable(WebServiceActivityBehavior.CURRENT_MESSAGE);
                if (message.StructureInstance is FieldBaseStructureInstance structure)
                {
                    structure.SetFieldValue(this.target, value);
                }
            }
        }
    }

}