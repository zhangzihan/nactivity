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

namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

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

        protected internal override object Execute(ICommandContext commandContext, IExecutionEntity execution)
        {
            if (processVariables is object)
            {
                execution.Variables = processVariables;
            }

            if (transientVariables is object)
            {
                execution.TransientVariables = transientVariables;
            }

            Context.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateSignalEvent(ActivitiEventType.ACTIVITY_SIGNALED, execution.CurrentActivityId, null, null, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId));

            Context.Agenda.PlanTriggerExecutionOperation(execution);

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