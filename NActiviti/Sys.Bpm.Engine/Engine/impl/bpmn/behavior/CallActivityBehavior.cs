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
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Delegate;
    using Sys.Workflow.Engine.Impl.EL;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Repository;


    /// <summary>
    /// Implementation of the BPMN 2.0 call activity (limited currently to calling a subprocess and not (yet) a global task).
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class CallActivityBehavior : SubProcessActivityBehavior, ISubProcessActivityBehavior
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

        public override void Execute(IExecutionEntity execution)
        {

            string finalProcessDefinitonKey;
            if (processDefinitionExpression != null)
            {
                finalProcessDefinitonKey = (string)processDefinitionExpression.GetValue(execution);
            }
            else
            {
                finalProcessDefinitonKey = processDefinitonKey;
            }

            IProcessDefinition processDefinition = FindProcessDefinition(finalProcessDefinitonKey, execution.TenantId);

            // Get model from cache
            Process subProcess = ProcessDefinitionUtil.GetProcess(processDefinition.Id);
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
            if (ProcessDefinitionUtil.IsProcessDefinitionSuspended(processDefinition.Id))
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
                IExpression expression = expressionManager.CreateExpression(callActivity.BusinessKey);
                businessKey = expression.GetValue(execution).ToString();

            }
            else if (callActivity.InheritBusinessKey)
            {
                IExecutionEntity processInstance = executionEntityManager.FindById<IExecutionEntity>(execution.ProcessInstanceId);
                businessKey = processInstance.BusinessKey;
            }

            IExecutionEntity subProcessInstance = Context.CommandContext.ExecutionEntityManager.CreateSubprocessInstance(processDefinition, execution, businessKey);
            Context.CommandContext.HistoryManager.RecordSubProcessInstanceStart(execution, subProcessInstance, initialFlowElement);

            // process template-defined data objects
            IDictionary<string, object> variables = ProcessDataObjects(subProcess.DataObjects);

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
                    IExpression expression = expressionManager.CreateExpression(ioParameter.SourceExpression.Trim());
                    value = expression.GetValue(execution);
                }
                else
                {
                    value = execution.GetVariable(ioParameter.Source);
                }
                variables[ioParameter.Target] = value;
            }

            if (variables.Count > 0)
            {
                InitializeVariables(subProcessInstance, variables);
            }

            // Create the first execution that will visit all the process definition elements
            IExecutionEntity subProcessInitialExecution = executionEntityManager.CreateChildExecution(subProcessInstance);
            subProcessInitialExecution.CurrentFlowElement = initialFlowElement;

            Context.Agenda.PlanContinueProcessOperation(subProcessInitialExecution);

            Context.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateProcessStartedEvent(subProcessInitialExecution, variables, false));

        }

        public virtual void Completing(IExecutionEntity execution, IExecutionEntity subProcessInstance)
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
                    IExpression expression = expressionManager.CreateExpression(ioParameter.SourceExpression.Trim());
                    value = expression.GetValue(subProcessInstance);
                }
                else
                {
                    value = subProcessInstance.GetVariable(ioParameter.Source);
                }
                execution.SetVariable(ioParameter.Target, value);
            }
        }

        public virtual void Completed(IExecutionEntity execution)
        {
            // only control flow. no sub process instance data available
            Leave(execution);
        }

        // Allow subclass to determine which version of a process to start.
        protected internal virtual IProcessDefinition FindProcessDefinition(string processDefinitionKey, string tenantId)
        {
            if (tenantId is null || ProcessEngineConfiguration.NO_TENANT_ID.Equals(tenantId))
            {
                return Context.ProcessEngineConfiguration.DeploymentManager.FindDeployedLatestProcessDefinitionByKey(processDefinitionKey);
            }
            else
            {
                return Context.ProcessEngineConfiguration.DeploymentManager.FindDeployedLatestProcessDefinitionByKeyAndTenantId(processDefinitionKey, tenantId);
            }
        }

        protected internal override IDictionary<string, object> ProcessDataObjects(ICollection<ValuedDataObject> dataObjects)
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
        protected internal virtual void InitializeVariables(IExecutionEntity subProcessInstance, IDictionary<string, object> variables)
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