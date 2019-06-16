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
namespace Sys.Workflow.Engine.Impl.Bpmn.Webservice
{
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Behavior;
    using Sys.Workflow.Engine.Impl.Bpmn.Datas;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// <summary>
    /// An implicit data output association between a source and a target. source is a property in the message and target is a variable in the current execution context
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class MessageImplicitDataOutputAssociation : AbstractDataAssociation
    {

        private const long serialVersionUID = 1L;

        public MessageImplicitDataOutputAssociation(string targetRef, IExpression sourceExpression) : base(sourceExpression, targetRef)
        {
        }

        public MessageImplicitDataOutputAssociation(string targetRef, string sourceRef) : base(sourceRef, targetRef)
        {
        }

        public override void Evaluate(IExecutionEntity execution)
        {
            MessageInstance message = (MessageInstance)execution.GetVariable(WebServiceActivityBehavior.CURRENT_MESSAGE);
            if (message.StructureInstance is FieldBaseStructureInstance)
            {
                FieldBaseStructureInstance structure = (FieldBaseStructureInstance)message.StructureInstance;
                execution.SetVariable(this.Target, structure.GetFieldValue(this.Source));
            }
        }
    }

}