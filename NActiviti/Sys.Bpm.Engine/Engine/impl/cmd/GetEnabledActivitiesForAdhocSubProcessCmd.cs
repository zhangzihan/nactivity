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
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    [Serializable]
    public class GetEnabledActivitiesForAdhocSubProcessCmd : ICommand<IList<FlowNode>>
    {

        private const long serialVersionUID = 1L;
        protected internal string executionId;

        public GetEnabledActivitiesForAdhocSubProcessCmd(string executionId)
        {
            this.executionId = executionId;
        }

        public  virtual IList<FlowNode>  Execute(ICommandContext commandContext)
        {
            IExecutionEntity execution = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(executionId);
            if (execution == null)
            {
                throw new ActivitiObjectNotFoundException("No execution found for id '" + executionId + "'", typeof(IExecutionEntity));
            }

            if (!(execution.CurrentFlowElement is AdhocSubProcess))
            {
                throw new ActivitiException("The current flow element of the requested execution is not an ad-hoc sub process");
            }

            IList<FlowNode> enabledFlowNodes = new List<FlowNode>();

            AdhocSubProcess adhocSubProcess = (AdhocSubProcess)execution.CurrentFlowElement;

            // if sequential ordering, only one child execution can be active, so no enabled activities
            if (adhocSubProcess.HasSequentialOrdering())
            {
                if (execution.Executions.Count > 0)
                {
                    return enabledFlowNodes;
                }
            }

            foreach (FlowElement flowElement in adhocSubProcess.FlowElements)
            {
                if (flowElement is FlowNode flowNode)
                {
                    if (flowNode.IncomingFlows.Count == 0)
                    {
                        enabledFlowNodes.Add(flowNode);
                    }
                }
            }

            return enabledFlowNodes;
        }

    }

}