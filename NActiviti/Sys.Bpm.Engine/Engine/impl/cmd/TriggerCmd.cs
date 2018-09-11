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

namespace org.activiti.engine.impl.cmd
{
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    /// 
    [Serializable]
    public class TriggerCmd : NeedsActiveExecutionCmd<object>
    {

        private const long serialVersionUID = 1L;

        protected internal IDictionary<string, object> processVariables;
        protected internal IDictionary<string, object> transientVariables;

        public TriggerCmd(string executionId, IDictionary<string, object> processVariables) : base(executionId)
        {
            this.processVariables = processVariables;
        }

        public TriggerCmd(string executionId, IDictionary<string, object> processVariables, IDictionary<string, object> transientVariables) : this(executionId, processVariables)
        {
            this.transientVariables = transientVariables;
        }

        protected internal override object execute(ICommandContext commandContext, IExecutionEntity execution)
        {
            if (processVariables != null)
            {
                execution.Variables = processVariables;
            }

            if (transientVariables != null)
            {
                execution.TransientVariables = transientVariables;
            }

            Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createSignalEvent(ActivitiEventType.ACTIVITY_SIGNALED, execution.CurrentActivityId, null, null, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId));

            Context.Agenda.planTriggerExecutionOperation(execution);

            return null;
        }

        protected internal override string SuspendedExceptionMessage
        {
            get
            {
                return "Cannot trigger an execution that is suspended";
            }
        }

    }

}