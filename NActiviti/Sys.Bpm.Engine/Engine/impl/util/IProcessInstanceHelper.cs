using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Engine.Runtime;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Util
{
	public interface IProcessInstanceHelper
	{
		IProcessInstance CreateAndStartProcessInstance(IProcessDefinition processDefinition, string businessKey, string processInstanceName, IDictionary<string, object> variables, IDictionary<string, object> transientVariables, string initialFlowElementId = null);
		IProcessInstance CreateAndStartProcessInstanceByMessage(IProcessDefinition processDefinition, string messageName, IDictionary<string, object> variables, IDictionary<string, object> transientVariables);
		IProcessInstance CreateAndStartProcessInstanceWithInitialFlowElement(IProcessDefinition processDefinition, string businessKey, string processInstanceName, FlowElement initialFlowElement, Process process, IDictionary<string, object> variables, IDictionary<string, object> transientVariables, bool startProcessInstance);
		IProcessInstance CreateProcessInstance(IProcessDefinitionEntity processDefinition, string businessKey, string processInstanceName, IDictionary<string, object> variables, IDictionary<string, object> transientVariables);
		void StartProcessInstance(IExecutionEntity processInstance, ICommandContext commandContext, IDictionary<string, object> variables);
	}
}