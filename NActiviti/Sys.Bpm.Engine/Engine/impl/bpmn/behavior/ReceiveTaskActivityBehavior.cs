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

namespace Sys.Workflow.engine.impl.bpmn.behavior
{
    using Sys.Workflow.engine.impl.persistence.entity;

    /// <summary>
    /// A receive task is a wait state that waits for the receival of some message.
    /// 
    /// Currently, the only message that is supported is the external trigger, given by calling the <seealso cref="IRuntimeService#signal(String)"/> operation.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ReceiveTaskActivityBehavior : TaskActivityBehavior
    {

        private const long serialVersionUID = 1L;

        public override void Execute(IExecutionEntity execution)
        {
            // Do nothing: waitstate behavior
        }

        public override void Trigger(IExecutionEntity execution, string signalName, object data, bool throwError = true)
        {
            Leave(execution);
        }

    }

}