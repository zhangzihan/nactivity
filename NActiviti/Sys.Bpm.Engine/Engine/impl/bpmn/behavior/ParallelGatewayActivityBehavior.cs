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

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow;


    /// <summary>
    /// Implementation of the Parallel Gateway/AND gateway as defined in the BPMN 2.0 specification.
    /// 
    /// The Parallel Gateway can be used for splitting a path of execution into multiple paths of executions (AND-split/fork behavior), one for every outgoing sequence flow.
    /// 
    /// The Parallel Gateway can also be used for merging or joining paths of execution (AND-join). In this case, on every incoming sequence flow an execution needs to arrive, before leaving the Parallel
    /// Gateway (and potentially then doing the fork behavior in case of multiple outgoing sequence flow).
    /// 
    /// Note that there is a slight difference to spec (p. 436): "The parallel gateway is activated if there is at least one Token on each incoming sequence flow." We only check the number of incoming
    /// tokens to the number of sequenceflow. So if two tokens would arrive through the same sequence flow, our implementation would activate the gateway.
    /// 
    /// Note that a Parallel Gateway having one incoming and multiple outgoing sequence flow, is the same as having multiple outgoing sequence flow on a given activity. However, a parallel gateway does NOT
    /// check conditions on the outgoing sequence flow.
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ParallelGatewayActivityBehavior : GatewayActivityBehavior
    {
        private static readonly ILogger<MultiInstanceActivityBehavior> log = ProcessEngineServiceProvider.LoggerService<MultiInstanceActivityBehavior>();

        private const long serialVersionUID = 1840892471343975524L;

        public override void Execute(IExecutionEntity execution)
        {

            // First off all, deactivate the execution
            execution.Inactivate();

            // Join
            FlowElement flowElement = execution.CurrentFlowElement;
            ParallelGateway parallelGateway;
            if (flowElement is ParallelGateway)
            {
                parallelGateway = (ParallelGateway)flowElement;
            }
            else
            {
                throw new ActivitiException("Programmatic error: parallel gateway behaviour can only be applied" + " to a ParallelGateway instance, but got an instance of " + flowElement);
            }

            LockFirstParentScope(execution);

            IExecutionEntity multiInstanceExecution = null;
            if (HasMultiInstanceParent(parallelGateway))
            {
                multiInstanceExecution = FindMultiInstanceParentExecution(execution);
            }

            IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;
            ICollection<IExecutionEntity> joinedExecutions = executionEntityManager.FindInactiveExecutionsByActivityIdAndProcessInstanceId(execution.CurrentActivityId, execution.ProcessInstanceId);
            if (multiInstanceExecution != null)
            {
                joinedExecutions = CleanJoinedExecutions(joinedExecutions, multiInstanceExecution);
            }

            int nbrOfExecutionsToJoin = parallelGateway.IncomingFlows.Count;
            int nbrOfExecutionsCurrentlyJoined = joinedExecutions.Count;

            // Fork

            // Is needed to set the endTime for all historic activity joins
            Context.CommandContext.HistoryManager.RecordActivityEnd(execution, null);

            if (nbrOfExecutionsCurrentlyJoined == nbrOfExecutionsToJoin)
            {

                // Fork
                if (log.IsEnabled(LogLevel.Debug))
                {
                    log.LogDebug($"parallel gateway '{execution.CurrentActivityId}' activates: {nbrOfExecutionsCurrentlyJoined} of {nbrOfExecutionsToJoin} joined");
                }

                if (parallelGateway.IncomingFlows.Count > 1)
                {
                    // All (now inactive) children are deleted.
                    foreach (IExecutionEntity joinedExecution in joinedExecutions)
                    {
                        // The current execution will be reused and not deleted
                        if (!joinedExecution.Id.Equals(execution.Id))
                        {
                            executionEntityManager.DeleteExecutionAndRelatedData(joinedExecution, null, false);
                        }

                    }
                }

                // TODO: potential optimization here: reuse more then 1 execution, only 1 currently
                Context.Agenda.PlanTakeOutgoingSequenceFlowsOperation(execution, false); // false -> ignoring conditions on parallel gw

            }
            else if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"parallel gateway '{execution.CurrentActivityId}' does not activate: {nbrOfExecutionsCurrentlyJoined} of {nbrOfExecutionsToJoin} joined");
            }

        }

        protected internal virtual ICollection<IExecutionEntity> CleanJoinedExecutions(ICollection<IExecutionEntity> joinedExecutions, IExecutionEntity multiInstanceExecution)
        {
            IList<IExecutionEntity> cleanedExecutions = new List<IExecutionEntity>();
            foreach (IExecutionEntity executionEntity in joinedExecutions)
            {
                if (IsChildOfMultiInstanceExecution(executionEntity, multiInstanceExecution))
                {
                    cleanedExecutions.Add(executionEntity);
                }
            }
            return cleanedExecutions;
        }

        protected internal virtual bool IsChildOfMultiInstanceExecution(IExecutionEntity executionEntity, IExecutionEntity multiInstanceExecution)
        {
            bool isChild = false;
            IExecutionEntity parentExecution = executionEntity.Parent;
            if (parentExecution != null)
            {
                if (parentExecution.Id.Equals(multiInstanceExecution.Id))
                {
                    isChild = true;
                }
                else
                {
                    bool isNestedChild = IsChildOfMultiInstanceExecution(parentExecution, multiInstanceExecution);
                    if (isNestedChild)
                    {
                        isChild = true;
                    }
                }
            }

            return isChild;
        }

        protected internal virtual bool HasMultiInstanceParent(FlowNode flowNode)
        {
            bool hasMultiInstanceParent = false;
            if (flowNode.SubProcess != null)
            {
                if (flowNode.SubProcess.LoopCharacteristics != null)
                {
                    hasMultiInstanceParent = true;
                }
                else
                {
                    bool hasNestedMultiInstanceParent = this.HasMultiInstanceParent(flowNode.SubProcess);
                    if (hasNestedMultiInstanceParent)
                    {
                        hasMultiInstanceParent = true;
                    }
                }
            }

            return hasMultiInstanceParent;
        }

        protected internal virtual IExecutionEntity FindMultiInstanceParentExecution(IExecutionEntity execution)
        {
            IExecutionEntity multiInstanceExecution = null;
            IExecutionEntity parentExecution = execution.Parent;
            if (parentExecution != null && parentExecution.CurrentFlowElement != null)
            {
                FlowElement flowElement = parentExecution.CurrentFlowElement;
                if (flowElement is Activity activity)
                {
                    if (activity.LoopCharacteristics != null)
                    {
                        multiInstanceExecution = parentExecution;
                    }
                }

                if (multiInstanceExecution == null)
                {
                    IExecutionEntity potentialMultiInstanceExecution = FindMultiInstanceParentExecution(parentExecution);
                    if (potentialMultiInstanceExecution != null)
                    {
                        multiInstanceExecution = potentialMultiInstanceExecution;
                    }
                }
            }

            return multiInstanceExecution;
        }

    }

}