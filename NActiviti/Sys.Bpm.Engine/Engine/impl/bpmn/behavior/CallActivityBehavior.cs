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
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.el;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.repository;


    /// <summary>
    /// Implementation of the BPMN 2.0 call activity (limited currently to calling a subprocess and not (yet) a global task).
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class CallActivityBehavior : SubProcessActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal string processDefinitonKey;
        protected internal IExpression processDefinitionExpression;
        protected internal IList<MapExceptionEntry> mapExceptions;

        public CallActivityBehavior(string processDefinitionKey, IList<MapExceptionEntry> mapExceptions)
        {
            this.processDefinitonKey = processDefinitionKey;
            this.mapExceptions = mapExceptions;
        }

        public CallActivityBehavior(IExpression processDefinitionExpression, IList<MapExceptionEntry> mapExceptions)
        {
            this.processDefinitionExpression = processDefinitionExpression;
            this.mapExceptions = mapExceptions;
        }

        public override void execute(IExecutionEntity execution)
        {

            string finalProcessDefinitonKey = null;
            if (processDefinitionExpression != null)
            {
                finalProcessDefinitonKey = (string)processDefinitionExpression.getValue(execution);
            }
            else
            {
                finalProcessDefinitonKey = processDefinitonKey;
            }

            IProcessDefinition processDefinition = findProcessDefinition(finalProcessDefinitonKey, execution.TenantId);

            // Get model from cache
            Process subProcess = ProcessDefinitionUtil.getProcess(processDefinition.Id);
            if (subProcess == null)
            {
                throw new ActivitiException("Cannot start a sub process instance. Process model " + processDefinition.Name + " (id = " + processDefinition.Id + ") could not be found");
            }

            FlowElement initialFlowElement = subProcess.InitialFlowElement;
            if (initialFlowElement == null)
            {
                throw new ActivitiException("No start element found for process definition " + processDefinition.Id);
            }

            // Do not start a process instance if the process definition is suspended
            if (ProcessDefinitionUtil.isProcessDefinitionSuspended(processDefinition.Id))
            {
                throw new ActivitiException("Cannot start process instance. Process definition " + processDefinition.Name + " (id = " + processDefinition.Id + ") is suspended");
            }

            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;
            ExpressionManager expressionManager = processEngineConfiguration.ExpressionManager;

            CallActivity callActivity = (CallActivity)execution.CurrentFlowElement;

            string businessKey = null;

            if (!string.IsNullOrWhiteSpace(callActivity.BusinessKey))
            {
                IExpression expression = expressionManager.createExpression(callActivity.BusinessKey);
                businessKey = expression.getValue(execution).ToString();

            }
            else if (callActivity.InheritBusinessKey)
            {
                IExecutionEntity processInstance = executionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", execution.ProcessInstanceId));
                businessKey = processInstance.BusinessKey;
            }

            IExecutionEntity subProcessInstance = Context.CommandContext.ExecutionEntityManager.createSubprocessInstance(processDefinition, execution, businessKey);
            Context.CommandContext.HistoryManager.recordSubProcessInstanceStart(execution, subProcessInstance, initialFlowElement);

            // process template-defined data objects
            IDictionary<string, object> variables = processDataObjects(subProcess.DataObjects);

            if (callActivity.InheritVariables)
            {
                IDictionary<string, object> executionVariables = execution.Variables;
                foreach (KeyValuePair<string, object> entry in executionVariables.SetOfKeyValuePairs())
                {
                    variables[entry.Key] = entry.Value;
                }
            }

            // copy process variables
            foreach (IOParameter ioParameter in callActivity.InParameters)
            {
                object value = null;
                if (!string.IsNullOrWhiteSpace(ioParameter.SourceExpression))
                {
                    IExpression expression = expressionManager.createExpression(ioParameter.SourceExpression.Trim());
                    value = expression.getValue(execution);

                }
                else
                {
                    value = execution.getVariable(ioParameter.Source);
                }
                variables[ioParameter.Target] = value;
            }

            if (variables.Count > 0)
            {
                initializeVariables(subProcessInstance, variables);
            }

            // Create the first execution that will visit all the process definition elements
            IExecutionEntity subProcessInitialExecution = executionEntityManager.createChildExecution(subProcessInstance);
            subProcessInitialExecution.CurrentFlowElement = initialFlowElement;

            Context.Agenda.planContinueProcessOperation(subProcessInitialExecution);

            Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createProcessStartedEvent(subProcessInitialExecution, variables, false));
        }

        public virtual void completing(IExecutionEntity execution, IExecutionEntity subProcessInstance)
        {
            // only data. no control flow available on this execution.

            ExpressionManager expressionManager = Context.ProcessEngineConfiguration.ExpressionManager;

            // copy process variables
            CallActivity callActivity = (CallActivity)execution.CurrentFlowElement;
            foreach (IOParameter ioParameter in callActivity.OutParameters)
            {
                object value = null;
                if (!string.IsNullOrWhiteSpace(ioParameter.SourceExpression))
                {
                    IExpression expression = expressionManager.createExpression(ioParameter.SourceExpression.Trim());
                    value = expression.getValue(subProcessInstance);

                }
                else
                {
                    value = subProcessInstance.getVariable(ioParameter.Source);
                }
                execution.setVariable(ioParameter.Target, value);
            }
        }

        public virtual void completed(IExecutionEntity execution)
        {
            // only control flow. no sub process instance data available
            leave(execution);
        }

        // Allow subclass to determine which version of a process to start.
        protected internal virtual IProcessDefinition findProcessDefinition(string processDefinitionKey, string tenantId)
        {
            if (string.ReferenceEquals(tenantId, null) || ProcessEngineConfiguration.NO_TENANT_ID.Equals(tenantId))
            {
                return Context.ProcessEngineConfiguration.DeploymentManager.findDeployedLatestProcessDefinitionByKey(processDefinitionKey);
            }
            else
            {
                return Context.ProcessEngineConfiguration.DeploymentManager.findDeployedLatestProcessDefinitionByKeyAndTenantId(processDefinitionKey, tenantId);
            }
        }

        protected internal override IDictionary<string, object> processDataObjects(ICollection<ValuedDataObject> dataObjects)
        {
            IDictionary<string, object> variablesMap = new Dictionary<string, object>();
            // convert data objects to process variables
            if (dataObjects != null)
            {
                variablesMap = new Dictionary<string, object>(dataObjects.Count);
                foreach (ValuedDataObject dataObject in dataObjects)
                {
                    variablesMap[dataObject.Name] = dataObject.Value;
                }
            }
            return variablesMap;
        }

        // Allow a subclass to override how variables are initialized.
        protected internal virtual void initializeVariables(IExecutionEntity subProcessInstance, IDictionary<string, object> variables)
        {
            subProcessInstance.Variables = variables;
        }

        public virtual string ProcessDefinitonKey
        {
            set
            {
                this.processDefinitonKey = value;
            }
            get
            {
                return processDefinitonKey;
            }
        }

    }

}