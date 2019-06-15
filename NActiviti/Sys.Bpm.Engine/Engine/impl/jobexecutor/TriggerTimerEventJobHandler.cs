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
namespace org.activiti.engine.impl.jobexecutor
{
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;


    /// 
    public class TriggerTimerEventJobHandler : IJobHandler
    {

        public const string TYPE = "trigger-timer";

        public virtual string Type
        {
            get
            {
                return TYPE;
            }
        }

        public virtual void Execute(IJobEntity job, string configuration, IExecutionEntity execution, ICommandContext commandContext)
        {

            Context.Agenda.PlanTriggerExecutionOperation(execution);

            if (commandContext.EventDispatcher.Enabled)
            {
                commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.TIMER_FIRED, job));
            }

            if (execution.CurrentFlowElement is BoundaryEvent)
            {
                IList<string> processedElements = new List<string>();
                DispatchExecutionTimeOut(job, execution, processedElements, commandContext);
            }
        }

        protected internal virtual void DispatchExecutionTimeOut(IJobEntity timerEntity, IExecutionEntity execution, IList<string> processedElements, ICommandContext commandContext)
        {
            FlowElement currentElement = execution.CurrentFlowElement;
            if (currentElement is BoundaryEvent)
            {
                BoundaryEvent boundaryEvent = (BoundaryEvent)execution.CurrentFlowElement;
                if (boundaryEvent.CancelActivity && boundaryEvent.AttachedToRef != null)
                {

                    if (!processedElements.Contains(boundaryEvent.Id))
                    {
                        processedElements.Add(boundaryEvent.Id);
                        IExecutionEntity parentExecution = execution.Parent;
                        DispatchExecutionTimeOut(timerEntity, parentExecution, processedElements, commandContext);
                    }
                }

            }
            else
            {

                // flow nodes
                if (execution.CurrentFlowElement is FlowNode)
                {
                    processedElements.Add(execution.CurrentActivityId);
                    DispatchActivityTimeOut(timerEntity, (FlowNode)execution.CurrentFlowElement, execution, commandContext);
                    if (execution.CurrentFlowElement is UserTask && !execution.IsMultiInstanceRoot)
                    {
                        IList<ITaskEntity> tasks = execution.Tasks;
                        if (tasks.Count > 0)
                        {
                            tasks[0].Canceled = true;
                        }
                    }
                }

                // subprocesses
                if (execution.CurrentFlowElement is SubProcess)
                {
                    foreach (IExecutionEntity subExecution in execution.Executions)
                    {
                        if (!processedElements.Contains(subExecution.CurrentActivityId))
                        {
                            DispatchExecutionTimeOut(timerEntity, subExecution, processedElements, commandContext);
                        }
                    }

                    // call activities
                }
                else if (execution.CurrentFlowElement is CallActivity)
                {
                    IExecutionEntity subProcessInstance = commandContext.ExecutionEntityManager.FindSubProcessInstanceBySuperExecutionId(execution.Id);
                    if (subProcessInstance != null)
                    {
                        IList<IExecutionEntity> childExecutions = subProcessInstance.Executions;
                        foreach (IExecutionEntity subExecution in childExecutions)
                        {
                            if (!processedElements.Contains(subExecution.CurrentActivityId))
                            {
                                DispatchExecutionTimeOut(timerEntity, subExecution, processedElements, commandContext);
                            }
                        }
                    }
                }
            }
        }

        protected internal virtual void DispatchActivityTimeOut(IJobEntity timerEntity, FlowNode flowNode, IExecutionEntity execution, ICommandContext commandContext)
        {
            commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateActivityCancelledEvent(flowNode.Id, flowNode.Name, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId, ParseActivityType(flowNode), timerEntity));
        }

        protected internal virtual string ParseActivityType(FlowNode flowNode)
        {
            string elementType = flowNode.GetType().Name;
            elementType = elementType.Substring(0, 1).ToLower() + elementType.Substring(1);
            return elementType;
        }
    }

}