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

namespace org.activiti.engine.impl.bpmn.helper
{
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;

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

        public static void propagateError(BpmnError error, IExecutionEntity execution)
        {
            propagateError(error.ErrorCode, execution);
        }

        public static void propagateError(string errorCode, IExecutionEntity execution)
        {
            IDictionary<string, IList<Event>> eventMap = findCatchingEventsForProcess(execution.ProcessDefinitionId, errorCode);
            if (eventMap.Count > 0)
            {
                executeCatch(eventMap, execution, errorCode);
            }
            else if (!execution.ProcessInstanceId.Equals(execution.RootProcessInstanceId))
            { // Call activity

                IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;
                IExecutionEntity processInstanceExecution = executionEntityManager.findById<IExecutionEntity>(execution.ProcessInstanceId);
                if (processInstanceExecution != null)
                {

                    IExecutionEntity parentExecution = processInstanceExecution.SuperExecution;

                    ISet<string> toDeleteProcessInstanceIds = new HashSet<string>();
                    toDeleteProcessInstanceIds.Add(execution.ProcessInstanceId);

                    while (parentExecution != null && eventMap.Count == 0)
                    {
                        eventMap = findCatchingEventsForProcess(parentExecution.ProcessDefinitionId, errorCode);
                        if (eventMap.Count > 0)
                        {

                            foreach (string processInstanceId in toDeleteProcessInstanceIds)
                            {
                                IExecutionEntity processInstanceEntity = executionEntityManager.findById<IExecutionEntity>(processInstanceId);

                                // Delete
                                executionEntityManager.deleteProcessInstanceExecutionEntity(processInstanceEntity.Id, execution.CurrentFlowElement != null ? execution.CurrentFlowElement.Id : null, "ERROR_EVENT " + errorCode, false, false);

                                // Event
                                if (Context.ProcessEngineConfiguration != null && Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
                                {
                                    Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.PROCESS_COMPLETED_WITH_ERROR_END_EVENT, processInstanceEntity));
                                }
                            }
                            executeCatch(eventMap, parentExecution, errorCode);

                        }
                        else
                        {
                            toDeleteProcessInstanceIds.Add(parentExecution.ProcessInstanceId);
                            IExecutionEntity superExecution = parentExecution.SuperExecution;
                            if (superExecution != null)
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

        protected internal static void executeCatch(IDictionary<string, IList<Event>> eventMap, IExecutionEntity delegateExecution, string errorId)
        {
            Event matchingEvent = null;
            IExecutionEntity currentExecution = delegateExecution;
            IExecutionEntity parentExecution = null;

            if ((eventMap?.ContainsKey(currentExecution.ActivityId)).GetValueOrDefault(false))
            {
                matchingEvent = eventMap[currentExecution.ActivityId][0];

                // Check for multi instance
                if (!ReferenceEquals(currentExecution.ParentId, null) && currentExecution.Parent.IsMultiInstanceRoot)
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
                while (matchingEvent == null && parentExecution != null)
                {
                    IFlowElementsContainer currentContainer = null;
                    if (parentExecution.CurrentFlowElement is IFlowElementsContainer)
                    {
                        currentContainer = (IFlowElementsContainer)parentExecution.CurrentFlowElement;
                    }
                    else if (parentExecution.Id.Equals(parentExecution.ProcessInstanceId))
                    {
                        currentContainer = ProcessDefinitionUtil.getProcess(parentExecution.ProcessDefinitionId);
                    }

                    foreach (string refId in eventMap.Keys)
                    {
                        IList<Event> events = eventMap[refId];
                        if (CollectionUtil.IsNotEmpty(events) && events[0] is StartEvent)
                        {
                            if (currentContainer.getFlowElement(refId) != null)
                            {
                                matchingEvent = events[0];
                            }
                        }
                    }

                    if (matchingEvent == null)
                    {
                        if ((eventMap?.ContainsKey(parentExecution.ActivityId)).GetValueOrDefault(false))
                        {
                            matchingEvent = eventMap[parentExecution.ActivityId][0];

                            // Check for multi instance
                            if (!ReferenceEquals(parentExecution.ParentId, null) && parentExecution.Parent.IsMultiInstanceRoot)
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

            if (matchingEvent != null && parentExecution != null)
            {
                executeEventHandler(matchingEvent, parentExecution, currentExecution, errorId);
            }
            else
            {
                throw new ActivitiException("No matching parent execution for error code " + errorId + " found");
            }
        }

        protected internal static void executeEventHandler(Event @event, IExecutionEntity parentExecution, IExecutionEntity currentExecution, string errorId)
        {
            if (Context.ProcessEngineConfiguration != null && Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                BpmnModel bpmnModel = ProcessDefinitionUtil.getBpmnModel(parentExecution.ProcessDefinitionId);
                if (bpmnModel != null)
                {

                    bpmnModel.Errors.TryGetValue(errorId, out string errorCode);
                    if (ReferenceEquals(errorCode, null))
                    {
                        errorCode = errorId;
                    }

                    Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createErrorEvent(ActivitiEventType.ACTIVITY_ERROR_RECEIVED, @event.Id, errorId, errorCode, parentExecution.Id, parentExecution.ProcessInstanceId, parentExecution.ProcessDefinitionId));
                }
            }

            if (@event is StartEvent)
            {
                IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;

                if (!currentExecution.ParentId.Equals(parentExecution.Id))
                {
                    Context.Agenda.planDestroyScopeOperation(currentExecution);
                }
                else
                {
                    executionEntityManager.deleteExecutionAndRelatedData(currentExecution, null, false);
                }

                IExecutionEntity eventSubProcessExecution = executionEntityManager.createChildExecution(parentExecution);
                eventSubProcessExecution.CurrentFlowElement = @event;
                Context.Agenda.planContinueProcessOperation(eventSubProcessExecution);

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

                Context.Agenda.planTriggerExecutionOperation(boundaryExecution);
            }
        }

        protected internal static IDictionary<string, IList<Event>> findCatchingEventsForProcess(string processDefinitionId, string errorCode)
        {
            IDictionary<string, IList<Event>> eventMap = new Dictionary<string, IList<Event>>();
            Process process = ProcessDefinitionUtil.getProcess(processDefinitionId);
            BpmnModel bpmnModel = ProcessDefinitionUtil.getBpmnModel(processDefinitionId);

            string compareErrorCode = retrieveErrorCode(bpmnModel, errorCode);

            IList<EventSubProcess> subProcesses = process.findFlowElementsOfType<EventSubProcess>(true);
            foreach (EventSubProcess eventSubProcess in subProcesses)
            {
                foreach (FlowElement flowElement in eventSubProcess.FlowElements)
                {
                    if (flowElement is StartEvent)
                    {
                        StartEvent startEvent = (StartEvent)flowElement;
                        if (CollectionUtil.IsNotEmpty(startEvent.EventDefinitions) && startEvent.EventDefinitions[0] is ErrorEventDefinition)
                        {
                            ErrorEventDefinition errorEventDef = (ErrorEventDefinition)startEvent.EventDefinitions[0];
                            string eventErrorCode = retrieveErrorCode(bpmnModel, errorEventDef.ErrorCode);

                            if (ReferenceEquals(eventErrorCode, null) || ReferenceEquals(compareErrorCode, null) || eventErrorCode.Equals(compareErrorCode))
                            {
                                IList<Event> startEvents = new List<Event>();
                                startEvents.Add(startEvent);
                                eventMap[eventSubProcess.Id] = startEvents;
                            }
                        }
                    }
                }
            }

            IList<BoundaryEvent> boundaryEvents = process.findFlowElementsOfType<BoundaryEvent>(true);
            foreach (BoundaryEvent boundaryEvent in boundaryEvents)
            {
                if (!ReferenceEquals(boundaryEvent.AttachedToRefId, null) && CollectionUtil.IsNotEmpty(boundaryEvent.EventDefinitions) && boundaryEvent.EventDefinitions[0] is ErrorEventDefinition)
                {

                    ErrorEventDefinition errorEventDef = (ErrorEventDefinition)boundaryEvent.EventDefinitions[0];
                    string eventErrorCode = retrieveErrorCode(bpmnModel, errorEventDef.ErrorCode);

                    if (ReferenceEquals(eventErrorCode, null) || ReferenceEquals(compareErrorCode, null) || eventErrorCode.Equals(compareErrorCode))
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

        public static bool mapException(Exception e, IExecutionEntity execution, IList<MapExceptionEntry> exceptionMap)
        {
            string errorCode = findMatchingExceptionMapping(e, exceptionMap);
            if (!ReferenceEquals(errorCode, null))
            {
                propagateError(errorCode, execution);
                return true;
            }
            else
            {
                IExecutionEntity callActivityExecution = null;
                IExecutionEntity parentExecution = execution.Parent;
                while (parentExecution != null && callActivityExecution == null)
                {
                    if (parentExecution.Id.Equals(parentExecution.ProcessInstanceId))
                    {
                        if (parentExecution.SuperExecution != null)
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

                if (callActivityExecution != null)
                {
                    CallActivity callActivity = (CallActivity)callActivityExecution.CurrentFlowElement;
                    if (CollectionUtil.IsNotEmpty(callActivity.MapExceptions))
                    {
                        errorCode = findMatchingExceptionMapping(e, callActivity.MapExceptions);
                        if (!ReferenceEquals(errorCode, null))
                        {
                            propagateError(errorCode, callActivityExecution);
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        protected internal static string findMatchingExceptionMapping(Exception e, IList<MapExceptionEntry> exceptionMap)
        {
            string defaultExceptionMapping = null;

            foreach (MapExceptionEntry me in exceptionMap)
            {
                string exceptionClass = me.ClassName;
                string errorCode = me.ErrorCode;

                // save the first mapping with no exception class as default map
                if (!string.IsNullOrWhiteSpace(errorCode) && string.IsNullOrWhiteSpace(exceptionClass) && ReferenceEquals(defaultExceptionMapping, null))
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
                    Type exceptionClassClass = ReflectUtil.loadClass(exceptionClass);
                    if (exceptionClassClass.IsAssignableFrom(e.GetType()))
                    {
                        return errorCode;
                    }
                }
            }

            return defaultExceptionMapping;
        }

        protected internal static string retrieveErrorCode(BpmnModel bpmnModel, string errorCode)
        {
            string finalErrorCode = null;
            if (!ReferenceEquals(errorCode, null) && bpmnModel.containsErrorRef(errorCode))
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