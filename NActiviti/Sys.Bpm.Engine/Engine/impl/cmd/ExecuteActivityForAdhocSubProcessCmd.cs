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

namespace org.activiti.engine.impl.cmd
{
    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;
    using System.Collections.Generic;

    /// 
    [Serializable]
    public class ExecuteActivityForAdhocSubProcessCmd : ICommand<IExecution>
    {

        private const long serialVersionUID = 1L;
        protected internal string executionId;
        protected internal string activityId;

        public ExecuteActivityForAdhocSubProcessCmd(string executionId, string activityId)
        {
            this.executionId = executionId;
            this.activityId = activityId;
        }

        public virtual IExecution execute(ICommandContext commandContext)
        {
            IExecutionEntity execution = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(executionId);
            if (execution == null)
            {
                throw new ActivitiObjectNotFoundException("No execution found for id '" + executionId + "'", typeof(IExecutionEntity));
            }

            if (!(execution.CurrentFlowElement is AdhocSubProcess))
            {
                throw new ActivitiException("The current flow element of the requested execution is not an ad-hoc sub process");
            }

            FlowNode foundNode = null;
            AdhocSubProcess adhocSubProcess = (AdhocSubProcess)execution.CurrentFlowElement;

            // if sequential ordering, only one child execution can be active
            if (adhocSubProcess.hasSequentialOrdering())
            {
                if (execution.Executions.Count > 0)
                {
                    throw new ActivitiException("Sequential ad-hoc sub process already has an active execution");
                }
            }

            foreach (FlowElement flowElement in adhocSubProcess.FlowElements)
            {
                if (activityId.Equals(flowElement.Id) && flowElement is FlowNode)
                {
                    FlowNode flowNode = (FlowNode)flowElement;
                    if (flowNode.IncomingFlows.Count == 0)
                    {
                        foundNode = flowNode;
                    }
                }
            }

            if (foundNode == null)
            {
                throw new ActivitiException("The requested activity with id " + activityId + " can not be enabled");
            }

            IExecutionEntity activityExecution = Context.CommandContext.ExecutionEntityManager.createChildExecution(execution);
            activityExecution.CurrentFlowElement = foundNode;
            Context.Agenda.planContinueProcessOperation(activityExecution);

            return activityExecution;
        }

    }

}