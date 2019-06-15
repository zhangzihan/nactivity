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
namespace org.activiti.engine.impl.bpmn.behavior
{
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.@delegate;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using Sys.Workflow;


    /// <summary>
    /// Implementation of the Inclusive Gateway/OR gateway/inclusive data-based gateway as defined in the BPMN specification.
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class InclusiveGatewayActivityBehavior : GatewayActivityBehavior, IInactiveActivityBehavior
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<InclusiveGatewayActivityBehavior>();

        private const long serialVersionUID = 1L;

        public override void Execute(IExecutionEntity execution)
        {
            // The join in the inclusive gateway works as follows:
            // When an execution enters it, it is inactivated.
            // All the inactivated executions stay in the inclusive gateway
            // until ALL executions that CAN reach the inclusive gateway have reached it.
            //
            // This check is repeated on execution changes until the inactivated
            // executions leave the gateway.

            execution.Inactivate();
            ExecuteInclusiveGatewayLogic(execution);
        }

        public virtual void ExecuteInactive(IExecutionEntity executionEntity)
        {
            ExecuteInclusiveGatewayLogic(executionEntity);
        }

        protected internal virtual void ExecuteInclusiveGatewayLogic(IExecutionEntity execution)
        {
            ICommandContext commandContext = Context.CommandContext;
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;

            LockFirstParentScope(execution);

            ICollection<IExecutionEntity> allExecutions = executionEntityManager.FindChildExecutionsByProcessInstanceId(execution.ProcessInstanceId);
            IEnumerator<IExecutionEntity> executionIterator = allExecutions.GetEnumerator();
            bool oneExecutionCanReachGateway = false;
            while (!oneExecutionCanReachGateway && executionIterator.MoveNext())
            {
                IExecutionEntity executionEntity = executionIterator.Current;
                if (!executionEntity.ActivityId.Equals(execution.CurrentActivityId))
                {
                    bool canReachGateway = ExecutionGraphUtil.IsReachable(execution.ProcessDefinitionId, executionEntity.ActivityId, execution.CurrentActivityId);
                    if (canReachGateway)
                    {
                        oneExecutionCanReachGateway = true;
                    }
                }
                else if (executionEntity.ActivityId.Equals(execution.CurrentActivityId) && executionEntity.IsActive)
                {
                    // Special case: the execution has reached the inc gw, but the operation hasn't been executed yet for that execution
                    oneExecutionCanReachGateway = true;
                }
            }

            // If no execution can reach the gateway, the gateway activates and executes fork behavior
            if (!oneExecutionCanReachGateway)
            {
                logger.LogDebug("Inclusive gateway cannot be reached by any execution and is activated");

                // Kill all executions here (except the incoming)
                ICollection<IExecutionEntity> executionsInGateway = executionEntityManager.FindInactiveExecutionsByActivityIdAndProcessInstanceId(execution.CurrentActivityId, execution.ProcessInstanceId);
                foreach (IExecutionEntity executionEntityInGateway in executionsInGateway)
                {
                    if (!executionEntityInGateway.Id.Equals(execution.Id))
                    {
                        commandContext.HistoryManager.RecordActivityEnd(executionEntityInGateway, null);
                        executionEntityManager.DeleteExecutionAndRelatedData(executionEntityInGateway, null, false);
                    }
                }

                // Leave
                commandContext.Agenda.PlanTakeOutgoingSequenceFlowsOperation(execution, true);
            }
        }
    }
}