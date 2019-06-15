using org.activiti.engine.impl.interceptor;
using org.activiti.engine.repository;
using org.activiti.engine.runtime;
using org.activiti.engine.task;
using System.Collections.Generic;

namespace org.activiti.engine.impl.cmd
{
    public class TerminateProcessInstanceCmd : ICommand<bool>
    {
        private readonly string processInstanceId;
        private readonly string reason;
        private readonly IDictionary<string, object> variables;

        public TerminateProcessInstanceCmd(string processInstanceId, string reason, IDictionary<string, object> variables = null)
        {
            this.processInstanceId = processInstanceId;
            this.reason = reason;
            this.variables = variables;
        }

        public bool Execute(ICommandContext commandContext)
        {
            ICommandExecutor commandExecutor = commandContext.ProcessEngineConfiguration.CommandExecutor;

            IProcessInstance processInstance = commandExecutor.Execute(new GetProcessInstanceByIdCmd(processInstanceId)) as IProcessInstance;
            if (processInstance is null)
            {
                throw new ActivitiException(string.Concat("Unable to find process instance for the given id: ", processInstanceId));
            }

            IProcessDefinition processDefinition = commandExecutor.Execute(new GetDeploymentProcessDefinitionCmd(processInstance.ProcessDefinitionId));
            if (processDefinition is null)
            {
                throw new ActivitiException(string.Concat("Unable to find process definition for the given id: ", processInstance.ProcessDefinitionId));
            }

            commandExecutor.Execute(new SetExecutionVariablesCmd(processInstance.Id, variables, false));

            string delReason = reason ?? "Cancelled";

            IList<ITask> tasks = commandExecutor.Execute(new GetTasksByProcessInstanceIdCmd(new string[] { processInstanceId }));

            foreach (var task in tasks)
            {
                commandExecutor.Execute(new AddCommentCmd(task.Id, processInstanceId, delReason));
            }

            commandExecutor.Execute(new DeleteProcessInstanceCmd(processInstanceId, delReason));

            return true;
        }
    }
}
