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
    public class CallActivityMultipleBusinessKeyListener : IExecutionListener
    {
        private static readonly string NUMBER_OF_EXECUTION_INSTANCES = "nrOfExecutionInstances";

        public void Notify(IExecutionEntity execution)
        {
            IExecutionEntity miRoot = execution.Parent;

            if (miRoot.IsMultiInstanceRoot
                && execution.CurrentFlowElement is CallActivity callActivity)
            {
                int nrOfExecInstances = ((int?)miRoot.GetVariableLocal(NUMBER_OF_EXECUTION_INSTANCES)).GetValueOrDefault(0);

                miRoot.SetVariableLocal(NUMBER_OF_EXECUTION_INSTANCES, nrOfExecInstances + 1);

                string collectionVarName = callActivity.LoopCharacteristics.GetCollectionVarName();

                string[] businessKeys = miRoot.GetVariable<JArray>(collectionVarName).ToObject<string[]>();

                execution.BusinessKey = businessKeys[nrOfExecInstances];
            }
        }
    }
}
