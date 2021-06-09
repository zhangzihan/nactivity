using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Delegate;
using Sys.Workflow.Engine.Delegate.Events;
using Sys.Workflow.Engine.Impl.Bpmn.Behavior;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.Persistence.Caches;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sys.Workflow.Engine.Impl.Bpmn.Listeners
{
    public class SubProcessActivityMultipleBusinessKeyListener : IExecutionListener
    {
        private static readonly string NUMBER_OF_EXECUTION_INSTANCES = "nrOfExecutionInstances";

        private static readonly string SUBPROCESS_BUSINESS_KEY = "{0}_BUSINESSKEY";

        public void Notify(IExecutionEntity execution)
        {
            SubProcess subProcess = GetSubProcessFromExecution(execution);

            if (subProcess.HasMultiInstanceLoopCharacteristics())
            {
                var varName = subProcess.LoopCharacteristics.GetCollectionVarName();

                if (string.IsNullOrWhiteSpace(varName) == false)
                {
                    IExecutionEntity miRoot = GetMiRootExecution(execution);
                    if (miRoot is null)
                    {
                        return;
                    }

                    string varBusiKey = string.Format(SUBPROCESS_BUSINESS_KEY, execution.Id);

                    if (GetVariable<object>(miRoot, varBusiKey) is object)
                    {
                        return;
                    }

                    int nrOfExecInstances = GetVariable<int?>(execution, NUMBER_OF_EXECUTION_INSTANCES).GetValueOrDefault(0);

                    miRoot.SetVariableLocal(NUMBER_OF_EXECUTION_INSTANCES, nrOfExecInstances + 1);
                    string[] ids = JsonConvert.DeserializeObject<string[]>(execution.GetVariable(varName).ToString());

                    execution.BusinessKey = ids[nrOfExecInstances];
                    miRoot.SetVariableLocal(varBusiKey, execution.BusinessKey);
                }
            }
        }

        private IExecutionEntity GetMiRootExecution(IExecutionEntity execution)
        {
            if (execution is null)
            {
                return null;
            }

            if (execution.IsMultiInstanceRoot)
            {
                return execution;
            }

            return GetMiRootExecution(execution.Parent);
        }

        private T GetVariable<T>(IExecutionEntity execution, string variableName)
        {
            object value = execution.GetVariableLocal(variableName);
            IExecutionEntity parent = execution.Parent;
            while (value is null && parent is object)
            {
                value = parent.GetVariableLocal(variableName);
                parent = parent.Parent;
            }
            return (T)value;
        }

        protected internal virtual SubProcess GetSubProcessFromExecution(IExecutionEntity execution)
        {
            FlowElement flowElement = execution.CurrentFlowElement;
            SubProcess subProcess;
            if (flowElement is SubProcess)
            {
                subProcess = (SubProcess)flowElement;
            }
            else
            {
                throw new ActivitiException("Programmatic error: sub process behaviour can only be applied" + " to a SubProcess instance, but got an instance of " + flowElement);
            }
            return subProcess;
        }
    }
}
