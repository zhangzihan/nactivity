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

namespace Sys.Workflow.Engine.Impl.Bpmn.Helper
{
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;

    /// <summary>
    /// This class is responsible for finding and executing error handlers for BPMN Errors.
    /// 
    /// Possible error handlers include Error Intermediate Events and Error Event Sub-Processes.
    /// 
    /// 
    /// 
    /// </summary>
    public class ErrorPropagation
    {

        public static void PropagateError(BpmnError error, IExecutionEntity execution)
        {
            PropagateError(error.ErrorCode, execution);
        }

        public static void PropagateError(string errorCode, IExecutionEntity execution)
        {
            IDictionary<string, IList<Event>> eventMap = FindCatchingEventsForProcess(execution.ProcessDefinitionId, errorCode);
            if (eventMap.Count > 0)
            {
                ExecuteCatch(eventMap, execution, errorCode);
            }
            else if (!execution.ProcessInstanceId.Equals(execution.RootProcessInstanceId))
            { // Call activity

                IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;
                IExecutionEntity processInstanceExecution = executionEntityManager.FindById<IExecutionEntity>(execution.ProcessInstanceId);
                if (processInstanceExecution is object)
                {
                    IExecutionEntity parentExecution = processInstanceExecution.SuperExecution;

                    IList<string> toDeleteProcessInstanceIds = new List<string>
                    {
                        execution.ProcessInstanceId
                    };

                    while (parentExecution is object && eventMap.Count == 0)
                    {
                        eventMap = FindCatchingEventsForProcess(parentExecution.ProcessDefinitionId, errorCode);
                        if (eventMap.Count > 0)
                        {

                            foreach (string processInstanceId in toDeleteProcessInstanceIds)
                            {
                                IExecutionEntity processInstanceEntity = executionEntityManager.FindById<IExecutionEntity>(processInstanceId);

                                // Delete
                                executionEntityManager.DeleteProcessInstanceExecutionEntity(processInstanceEntity.Id, execution.CurrentFlowElement?.Id, "ERROR_EVENT " + errorCode, false, false);

                                // Event
                                ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
                                if (processEngineConfiguration is not null && Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
                                {
                                    processEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.PROCESS_COMPLETED_WITH_ERROR_END_EVENT, processInstanceEntity));
                                }
                            }
                            ExecuteCatch(eventMap, parentExecution, errorCode);
                        }
                        else
                        {
                            toDeleteProcessInstanceIds.Add(parentExecution.ProcessInstanceId);
                            IExecutionEntity superExecution = parentExecution.SuperExecution;
                            if (superExecution is object)
                            {
                                parentExecution = superExecution;
                            }
                            else if (!parentExecution.Id.Equals(parentExecution.RootProcessInstanceId))
                            { // stop at the root
                                parentExecution = parentExecution.ProcessInstance;
                            }
                            else
                            {
                                parentExecution = null;
                            }
                        }
                    }
                }
            }

            if (eventMap.Count == 0)
            {
                throw new BpmnError(errorCode, "No catching boundary event found for error with errorCode '" + errorCode + "', neither in same process nor in parent process");
            }
        }

        protected internal static void ExecuteCatch(IDictionary<string, IList<Event>> eventMap, IExecutionEntity delegateExecution, string errorId)
        {
            Event matchingEvent = null;
            IExecutionEntity currentExecution = delegateExecution;
            IExecutionEntity parentExecution;
            if ((eventMap?.ContainsKey(currentExecution.ActivityId)).GetValueOrDefault(false))
            {
                matchingEvent = eventMap[currentExecution.ActivityId][0];

                // Check for multi instance
                if (currentExecution.ParentId is not null && currentExecution.Parent.IsMultiInstanceRoot)
                {
                    parentExecution = currentExecution.Parent;
                }
                else
                {
                    parentExecution = currentExecution;
                }

            }
            else
            {
                parentExecution = currentExecution.Parent;

                // Traverse parents until one is found that is a scope and matches the activity the boundary event is defined on
                while (matchingEvent is null && parentExecution is object)
                {
                    IFlowElementsContainer currentContainer = null;
                    if (parentExecution.CurrentFlowElement is IFlowElementsContainer)
                    {
                        currentContainer = (IFlowElementsContainer)parentExecution.CurrentFlowElement;
                    }
                    else if (parentExecution.Id.Equals(parentExecution.ProcessInstanceId))
                    {
                        currentContainer = ProcessDefinitionUtil.GetProcess(parentExecution.ProcessDefinitionId);
                    }

                    foreach (string refId in eventMap.Keys)
                    {
                        IList<Event> events = eventMap[refId];
                        if (CollectionUtil.IsNotEmpty(events) && events[0] is StartEvent)
                        {
                            if (currentContainer.FindFlowElement(refId) is not null)
                            {
                                matchingEvent = events[0];
                            }
                        }
                    }

                    if (matchingEvent is null)
                    {
                        if ((eventMap?.ContainsKey(parentExecution.ActivityId)).GetValueOrDefault(false))
                        {
                            matchingEvent = eventMap[parentExecution.ActivityId][0];

                            // Check for multi instance
                            if (parentExecution.ParentId is not null && parentExecution.Parent.IsMultiInstanceRoot)
                            {
                                parentExecution = parentExecution.Parent;
                            }

                        }
                        else if (!string.IsNullOrWhiteSpace(parentExecution.ParentId))
                        {
                            parentExecution = parentExecution.Parent;
                        }
                        else
                        {
                            parentExecution = null;
                        }
                    }
                }
            }

            if (matchingEvent is not null && parentExecution is object)
            {
                ExecuteEventHandler(matchingEvent, parentExecution, currentExecution, errorId);
            }
            else
            {
                throw new ActivitiException("No matching parent execution for error code " + errorId + " found");
            }
        }

        protected internal static void ExecuteEventHandler(Event @event, IExecutionEntity parentExecution, IExecutionEntity currentExecution, string errorId)
        {
            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            if (processEngineConfiguration is not null && processEngineConfiguration.EventDispatcher.Enabled)
            {
                BpmnModel bpmnModel = ProcessDefinitionUtil.GetBpmnModel(parentExecution.ProcessDefinitionId);
                if (bpmnModel is not null)
                {
                    bpmnModel.Errors.TryGetValue(errorId, out string errorCode);
                    if (errorCode is null)
                    {
                        errorCode = errorId;
                    }

                    processEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateErrorEvent(ActivitiEventType.ACTIVITY_ERROR_RECEIVED, @event.Id, errorId, errorCode, parentExecution.Id, parentExecution.ProcessInstanceId, parentExecution.ProcessDefinitionId));
                }
            }

            if (@event is StartEvent)
            {
                IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;

                if (!currentExecution.ParentId.Equals(parentExecution.Id))
                {
                    Context.Agenda.PlanDestroyScopeOperation(currentExecution);
                }
                else
                {
                    executionEntityManager.DeleteExecutionAndRelatedData(currentExecution, null, false);
                }

                IExecutionEntity eventSubProcessExecution = executionEntityManager.CreateChildExecution(parentExecution);
                eventSubProcessExecution.CurrentFlowElement = @event;
                Context.Agenda.PlanContinueProcessOperation(eventSubProcessExecution);
            }
            else
            {
                IExecutionEntity boundaryExecution = null;
                IList<IExecutionEntity> childExecutions = parentExecution.Executions;
                foreach (IExecutionEntity childExecution in childExecutions)
                {
                    if (childExecution.ActivityId.Equals(@event.Id))
                    {
                        boundaryExecution = childExecution;
                    }
                }

                Context.Agenda.PlanTriggerExecutionOperation(boundaryExecution);
            }
        }

        protected internal static IDictionary<string, IList<Event>> FindCatchingEventsForProcess(string processDefinitionId, string errorCode)
        {
            IDictionary<string, IList<Event>> eventMap = new Dictionary<string, IList<Event>>();
            Process process = ProcessDefinitionUtil.GetProcess(processDefinitionId);
            BpmnModel bpmnModel = ProcessDefinitionUtil.GetBpmnModel(processDefinitionId);

            string compareErrorCode = RetrieveErrorCode(bpmnModel, errorCode);

            IList<EventSubProcess> subProcesses = process.FindFlowElementsOfType<EventSubProcess>(true);
            foreach (EventSubProcess eventSubProcess in subProcesses)
            {
                foreach (FlowElement flowElement in eventSubProcess.FlowElements)
                {
                    if (flowElement is StartEvent startEvent)
                    {
                        if (CollectionUtil.IsNotEmpty(startEvent.EventDefinitions) && startEvent.EventDefinitions[0] is ErrorEventDefinition)
                        {
                            ErrorEventDefinition errorEventDef = (ErrorEventDefinition)startEvent.EventDefinitions[0];
                            string eventErrorCode = RetrieveErrorCode(bpmnModel, errorEventDef.ErrorCode);

                            if (eventErrorCode is null || compareErrorCode is null || eventErrorCode.Equals(compareErrorCode))
                            {
                                IList<Event> startEvents = new List<Event>
                                {
                                    startEvent
                                };
                                eventMap[eventSubProcess.Id] = startEvents;
                            }
                        }
                    }
                }
            }

            IList<BoundaryEvent> boundaryEvents = process.FindFlowElementsOfType<BoundaryEvent>(true);
            foreach (BoundaryEvent boundaryEvent in boundaryEvents)
            {
                if (boundaryEvent.AttachedToRefId is not null && CollectionUtil.IsNotEmpty(boundaryEvent.EventDefinitions) && boundaryEvent.EventDefinitions[0] is ErrorEventDefinition)
                {

                    ErrorEventDefinition errorEventDef = (ErrorEventDefinition)boundaryEvent.EventDefinitions[0];
                    string eventErrorCode = RetrieveErrorCode(bpmnModel, errorEventDef.ErrorCode);

                    if (eventErrorCode is null || compareErrorCode is null || eventErrorCode.Equals(compareErrorCode))
                    {
                        IList<Event> elementBoundaryEvents = null;
                        if (!eventMap.ContainsKey(boundaryEvent.AttachedToRefId))
                        {
                            elementBoundaryEvents = new List<Event>();
                            eventMap[boundaryEvent.AttachedToRefId] = elementBoundaryEvents;
                        }
                        else
                        {
                            elementBoundaryEvents = eventMap[boundaryEvent.AttachedToRefId];
                        }
                        elementBoundaryEvents.Add(boundaryEvent);
                    }
                }
            }
            return eventMap;
        }

        public static bool MapException(Exception e, IExecutionEntity execution, IList<MapExceptionEntry> exceptionMap)
        {
            string errorCode = FindMatchingExceptionMapping(e, exceptionMap);
            if (errorCode is not null)
            {
                PropagateError(errorCode, execution);
                return true;
            }
            else
            {
                IExecutionEntity callActivityExecution = null;
                IExecutionEntity parentExecution = execution.Parent;
                while (parentExecution is object && callActivityExecution is null)
                {
                    if (parentExecution.Id.Equals(parentExecution.ProcessInstanceId))
                    {
                        if (parentExecution.SuperExecution is object)
                        {
                            callActivityExecution = parentExecution.SuperExecution;
                        }
                        else
                        {
                            parentExecution = null;
                        }
                    }
                    else
                    {
                        parentExecution = parentExecution.Parent;
                    }
                }

                if (callActivityExecution is object)
                {
                    CallActivity callActivity = (CallActivity)callActivityExecution.CurrentFlowElement;
                    if (CollectionUtil.IsNotEmpty(callActivity.MapExceptions))
                    {
                        errorCode = FindMatchingExceptionMapping(e, callActivity.MapExceptions);
                        if (errorCode is not null)
                        {
                            PropagateError(errorCode, callActivityExecution);
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        protected internal static string FindMatchingExceptionMapping(Exception e, IList<MapExceptionEntry> exceptionMap)
        {
            string defaultExceptionMapping = null;

            foreach (MapExceptionEntry me in exceptionMap)
            {
                string exceptionClass = me.ClassName;
                string errorCode = me.ErrorCode;

                // save the first mapping with no exception class as default map
                if (!string.IsNullOrWhiteSpace(errorCode) && string.IsNullOrWhiteSpace(exceptionClass) && defaultExceptionMapping is null)
                {
                    defaultExceptionMapping = errorCode;
                    continue;
                }

                // ignore if error code or class are not defined
                if (string.IsNullOrWhiteSpace(errorCode) || string.IsNullOrWhiteSpace(exceptionClass))
                {
                    continue;
                }

                if (e.GetType().FullName.Equals(exceptionClass))
                {
                    return errorCode;
                }
                if (me.AndChildren)
                {
                    Type exceptionClassClass = ReflectUtil.LoadClass(exceptionClass);
                    if (exceptionClassClass.IsAssignableFrom(e.GetType()))
                    {
                        return errorCode;
                    }
                }
            }

            return defaultExceptionMapping;
        }

        protected internal static string RetrieveErrorCode(BpmnModel bpmnModel, string errorCode)
        {
            string finalErrorCode = null;
            if (errorCode is not null && bpmnModel.ContainsErrorRef(errorCode))
            {
                finalErrorCode = bpmnModel.Errors[errorCode];
            }
            else
            {
                finalErrorCode = errorCode;
            }
            return finalErrorCode;
        }
    }
}