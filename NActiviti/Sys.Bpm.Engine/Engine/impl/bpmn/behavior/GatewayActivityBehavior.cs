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
namespace org.activiti.engine.impl.bpmn.behavior
{
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.persistence.entity;
    using System.Collections.Generic;

    /// <summary>
    /// super class for all gateway activity implementations.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public abstract class GatewayActivityBehavior : FlowNodeActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal virtual void lockFirstParentScope(IExecutionEntity execution)
        {

            IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;

            bool found = false;
            IExecutionEntity parentScopeExecution = null;
            while (!found && execution != null && !ReferenceEquals(execution.ParentId, null))
            {
                parentScopeExecution = executionEntityManager.findById<IExecutionEntity>(execution.ParentId);
                if (parentScopeExecution != null && parentScopeExecution.IsScope)
                {
                    found = true;
                }
                execution = parentScopeExecution;
            }

            parentScopeExecution.forceUpdate();
        }

    }

}