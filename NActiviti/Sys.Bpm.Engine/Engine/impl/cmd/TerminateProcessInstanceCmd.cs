using Microsoft.Extensions.Logging;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Engine.Tasks;
using System;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Cmd
{
    public class TerminateProcessInstanceCmd : ICommand<bool>
    {
        private readonly string processInstanceId;
        private readonly string reason;
        private readonly IDictionary<string, object> variables;
        private readonly string businessKey;
        private readonly ILogger<TerminateProcessInstanceCmd> logger = ProcessEngineServiceProvider.LoggerService<TerminateProcessInstanceCmd>();

        public TerminateProcessInstanceCmd(string processInstanceId, string businessKey, string reason, IDictionary<string, object> variables = null)
        {
            this.processInstanceId = processInstanceId;
            this.businessKey = businessKey;
            this.reason = reason;
            this.variables = variables;
        }

        public bool Execute(ICommandContext commandContext)
        {
            try
            {
                ICommandExecutor commandExecutor = commandContext.ProcessEngineConfiguration.CommandExecutor;

                List<IProcessInstance> processInstances = new List<IProcessInstance>();

                if (string.IsNullOrWhiteSpace(processInstanceId) == false)
                {
                    if (!(commandExecutor.Execute(new GetProcessInstanceByIdCmd(processInstanceId)) is IProcessInstance processInstance))
                    {
                        throw new ActivitiObjectNotFoundException(string.Concat("Unable to find process instance for the given id: ", processInstanceId));
                    }

                    processInstances.Add(processInstance);
                }
                else if (string.IsNullOrWhiteSpace(businessKey) == false)
                {
                    IRuntimeService runtimeService = commandContext.ProcessEngineConfiguration.RuntimeService;

                    IProcessInstanceQuery query = runtimeService.CreateProcessInstanceQuery()
                        .SetProcessInstanceBusinessKey(businessKey);

                    if (query is ProcessInstanceQueryImpl pqi)
                    {
                        pqi.IncludeChildExecutionsWithBusinessKeyQuery = false;
                    }

                    var list = commandExecutor.Execute(new GetProcessInstancesCmd(query, 0, int.MaxValue));

                    if (list.Count == 0)
                    {
                        throw new ActivitiObjectNotFoundException(string.Concat("Unable to find process instance for the given businessKey: ", businessKey));
                    }

                    processInstances.AddRange(list);
                }

                foreach (IProcessInstance processInstance in processInstances)
                {
                    IProcessDefinition processDefinition = commandExecutor.Execute(new GetDeploymentProcessDefinitionCmd(processInstance.ProcessDefinitionId));
                    if (processDefinition is null)
                    {
                        throw new ActivitiObjectNotFoundException(string.Concat("Unable to find process definition for the given id: ", processInstance.ProcessDefinitionId));
                    }

                    commandExecutor.Execute(new SetExecutionVariablesCmd(processInstance.Id, variables, false));

                    string delReason = reason ?? "Cancelled";

                    IList<ITask> tasks = commandExecutor.Execute(new GetTasksByProcessInstanceIdCmd(new string[] { processInstance.Id }));

                    foreach (var task in tasks)
                    {
                        commandExecutor.Execute(new AddCommentCmd(task.Id, processInstance.Id, delReason));
                    }

                    commandExecutor.Execute(new DeleteProcessInstanceCmd(processInstance.Id, delReason));
                }
            }
            catch (NullReferenceException ex)
            {
                logger.LogError($"终止流程失败：{ex.StackTrace}");
                throw;
            }

            return true;
        }
    }
}
