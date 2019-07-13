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
namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util.Conditions;
    using Sys.Workflow;

    /// <summary>
    /// implementation of the Exclusive Gateway/XOR gateway/exclusive data-based gateway as defined in the BPMN specification.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ExclusiveGatewayActivityBehavior : GatewayActivityBehavior
    {
        private static readonly ILogger<ExclusiveGatewayActivityBehavior> log = ProcessEngineServiceProvider.LoggerService<ExclusiveGatewayActivityBehavior>();

        private const long serialVersionUID = 1L;

        // private static Logger log = LoggerFactory.getLogger(typeof(ExclusiveGatewayActivityBehavior));

        /// <summary>
        /// The default behaviour of BPMN, taking every outgoing sequence flow (where the condition evaluates to true), is not valid for an exclusive gateway.
        /// 
        /// Hence, this behaviour is overridden and replaced by the correct behavior: selecting the first sequence flow which condition evaluates to true (or which hasn't got a condition) and leaving the
        /// activity through that sequence flow.
        /// 
        /// If no sequence flow is selected (ie all conditions evaluate to false), then the default sequence flow is taken (if defined).
        /// </summary>
        public override void Leave(IExecutionEntity execution)
        {
            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"Leaving exclusive gateway '{execution.CurrentActivityId}'");
            }

            ExclusiveGateway exclusiveGateway = (ExclusiveGateway)execution.CurrentFlowElement;

            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            if (processEngineConfiguration is object && processEngineConfiguration.EventDispatcher.Enabled)
            {
                processEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateActivityEvent(ActivitiEventType.ACTIVITY_COMPLETED, exclusiveGateway.Id, exclusiveGateway.Name, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId, exclusiveGateway));
            }

            SequenceFlow outgoingSequenceFlow = null;
            SequenceFlow defaultSequenceFlow = null;
            string defaultSequenceFlowId = exclusiveGateway.DefaultFlow;

            // Determine sequence flow to take
            foreach (SequenceFlow sequenceFlow in exclusiveGateway.OutgoingFlows)
            {
                string skipExpressionString = sequenceFlow.SkipExpression;
                if (!SkipExpressionUtil.IsSkipExpressionEnabled(execution, skipExpressionString))
                {
                    bool conditionEvaluatesToTrue = ConditionUtil.HasTrueCondition(sequenceFlow, execution);
                    if (conditionEvaluatesToTrue && (defaultSequenceFlowId is null || !defaultSequenceFlowId.Equals(sequenceFlow.Id)))
                    {
                        if (log.IsEnabled(LogLevel.Debug))
                        {
                            log.LogDebug($"Sequence flow '{sequenceFlow.Id}'selected as outgoing sequence flow.");
                        }
                        outgoingSequenceFlow = sequenceFlow;
                        break;
                    }
                }
                else if (SkipExpressionUtil.ShouldSkipFlowElement(Context.CommandContext, execution, skipExpressionString))
                {
                    outgoingSequenceFlow = sequenceFlow;
                    break;
                }

                // Already store it, if we would need it later. Saves one for loop.
                if (defaultSequenceFlowId is object && defaultSequenceFlowId.Equals(sequenceFlow.Id))
                {
                    defaultSequenceFlow = sequenceFlow;
                }

            }

            // We have to record the end here, or else we're already past it
            Context.CommandContext.HistoryManager.RecordActivityEnd(execution, null);

            // Leave the gateway
            if (outgoingSequenceFlow != null)
            {
                execution.CurrentFlowElement = outgoingSequenceFlow;

                log.LogInformation($"条件表达式:id={outgoingSequenceFlow.Id} expression={outgoingSequenceFlow.ConditionExpression}");
            }
            else
            {
                if (defaultSequenceFlow != null)
                {
                    execution.CurrentFlowElement = defaultSequenceFlow;

                    log.LogInformation($"条件表达式:id={defaultSequenceFlow.Id} expression={defaultSequenceFlow.ConditionExpression}");
                }
                else
                {

                    // No sequence flow could be found, not even a default one
                    throw new ActivitiException("No outgoing sequence flow of the exclusive gateway '" + exclusiveGateway.Id + "' could be selected for continuing the process");
                }
            }

            base.Leave(execution);
        }
    }

}