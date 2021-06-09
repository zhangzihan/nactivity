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
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;


    /// 
    [Serializable]
    public class CompleteAdhocSubProcessCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;

        protected internal string executionId;

        public CompleteAdhocSubProcessCmd(string executionId)
        {
            this.executionId = executionId;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
            IExecutionEntity execution = executionEntityManager.FindById<IExecutionEntity>(executionId);
            if (execution is null)
            {
                throw new ActivitiObjectNotFoundException("No execution found for id '" + executionId + "'", typeof(IExecutionEntity));
            }

            if (!(execution.CurrentFlowElement is AdhocSubProcess))
            {
                throw new ActivitiException("The current flow element of the requested execution is not an ad-hoc sub process");
            }

            IList<IExecutionEntity> childExecutions = execution.Executions;
            if (childExecutions.Count > 0)
            {
                throw new ActivitiException("Ad-hoc sub process has running child executions that need to be completed first");
            }

            IExecutionEntity outgoingFlowExecution = executionEntityManager.CreateChildExecution(execution.Parent);
            outgoingFlowExecution.CurrentFlowElement = execution.CurrentFlowElement;

            executionEntityManager.DeleteExecutionAndRelatedData(execution, null, false);

            Context.Agenda.PlanTakeOutgoingSequenceFlowsOperation(outgoingFlowExecution, true);

            return null;
        }

    }

}