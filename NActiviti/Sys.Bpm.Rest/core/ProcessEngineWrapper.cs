using Microsoft.Extensions.Logging;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Bpmn.Constants;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Api.Model.Converters;
using Sys.Workflow.Cloud.Services.Core.Pageables;
using Sys.Workflow.Cloud.Services.Events.Listeners;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.History;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Engine.Tasks;
using Sys.Workflow.Services.Api.Commands;
using Sys.Workflow.Contexts;
using Sys.Workflow;
using System.Collections.Generic;
using System.Linq;

namespace Sys.Workflow.Cloud.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessEngineWrapper
    {
        private readonly ILogger logger = null;

        private readonly ProcessInstanceConverter processInstanceConverter;
        private readonly IRuntimeService runtimeService;
        private readonly ITaskService taskService;
        private readonly TaskConverter taskConverter;
        private readonly PageableTaskRepositoryService pageableTaskService;
        private readonly SecurityPoliciesApplicationService securityService;
        private readonly IRepositoryService repositoryService;
        private readonly AuthenticationWrapper authenticationWrapper;
        private PageableProcessInstanceRepositoryService pageableProcessInstanceService;
        private readonly IApplicationEventPublisher eventPublisher;
        private readonly IProcessEngine processEngine;
        private readonly IHistoryService historyService;
        private readonly HistoricInstanceConverter historicInstanceConverter;

        /// <summary>
        /// 
        /// </summary>
        public ProcessEngineWrapper(ProcessInstanceConverter processInstanceConverter,
            PageableProcessInstanceRepositoryService pageableProcessInstanceService,
            TaskConverter taskConverter,
            PageableTaskRepositoryService pageableTaskService,
            MessageProducerActivitiEventListener listener,
            SecurityPoliciesApplicationService securityService,
            AuthenticationWrapper authenticationWrapper,
            IApplicationEventPublisher eventPublisher,
            IProcessEngine processEngine,
            HistoricInstanceConverter historicInstanceConverter,
            ILoggerFactory loggerFactory)
        {
            this.processEngine = processEngine;
            this.processInstanceConverter = processInstanceConverter;
            this.runtimeService = processEngine.RuntimeService;
            this.pageableProcessInstanceService = pageableProcessInstanceService;
            this.taskService = processEngine.TaskService;
            this.taskConverter = taskConverter;
            this.pageableTaskService = pageableTaskService;
            this.historyService = processEngine.HistoryService;
#warning 暂时不处理事件侦听
            //this.runtimeService.addEventListener(listener);
            this.securityService = securityService;
            this.repositoryService = processEngine.RepositoryService;
            this.authenticationWrapper = authenticationWrapper;
            this.eventPublisher = eventPublisher;
            this.historicInstanceConverter = historicInstanceConverter;
            logger = loggerFactory.CreateLogger<ProcessEngineWrapper>();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<ProcessInstance> GetProcessInstances(Pageable pageable)
        {
            return pageableProcessInstanceService.GetProcessInstances(pageable);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<ProcessInstance> GetAllProcessInstances(Pageable pageable)
        {
            return pageableProcessInstanceService.GetAllProcessInstances(pageable);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessInstance[] StartProcess(IStartProcessInstanceCmd[] cmds)
        {
            //if (!securityService.canWrite(processDefinitionKey))
            //{
            //    LOGGER.debug("User " + authenticationWrapper.AuthenticatedUserId + " not permitted to access definition " + processDefinitionKey);
            //    throw new ActivitiForbiddenException("Operation not permitted for " + processDefinitionKey);
            //}

            IProcessInstance[] processInstance = runtimeService.StartProcessInstanceByCmd(cmds);

            return processInstanceConverter.From(processInstance).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Signal(SignalCmd signalCmd)
        {
            //TODO: plan is to restrict access to events using a new security policy on events
            // - that's another piece of work though so for now no security here

            runtimeService.SignalEventReceived(signalCmd.Name, signalCmd.InputVariables);
            eventPublisher.PublishEvent(signalCmd);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessInstance Suspend(SuspendProcessInstanceCmd suspendProcessInstanceCmd)
        {
            VerifyCanWriteToProcessInstance(suspendProcessInstanceCmd.ProcessInstanceId);
            runtimeService.SuspendProcessInstanceById(suspendProcessInstanceCmd.ProcessInstanceId);

            return GetProcessInstanceById(suspendProcessInstanceCmd.ProcessInstanceId);
        }

        private void VerifyCanWriteToProcessInstance(string processInstanceId)
        {
            ProcessInstance processInstance = GetProcessInstanceById(processInstanceId);
            if (processInstance == null)
            {
                throw new ActivitiException("Unable to find process instance for the given id: " + processInstanceId);
            }

            IProcessDefinition processDefinition = repositoryService.GetProcessDefinition(processInstance.ProcessDefinitionId);
            if (processDefinition == null)
            {
                throw new ActivitiException("Unable to find process definition for the given id: " + processInstance.ProcessDefinitionId);
            }

            //if (!securityService.canWrite(processDefinition.Key))
            //{
            //    LOGGER.debug("User " + authenticationWrapper.AuthenticatedUserId + " not permitted to access definition " + processDefinition.Key);
            //    throw new ActivitiForbiddenException("Operation not permitted for " + processDefinition.Key);
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessInstance Activate(ActivateProcessInstanceCmd activateProcessInstanceCmd)
        {
            VerifyCanWriteToProcessInstance(activateProcessInstanceCmd.ProcessInstanceId);
            runtimeService.ActivateProcessInstanceById(activateProcessInstanceCmd.ProcessInstanceId);

            return GetProcessInstanceById(activateProcessInstanceCmd.ProcessInstanceId);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessInstance GetProcessInstanceById(string processInstanceId)
        {
            IProcessInstance processInstance = (runtimeService as ServiceImpl).CommandExecutor.Execute(new Engine.Impl.Cmd.GetProcessInstanceByIdCmd(processInstanceId));

            return processInstanceConverter.From(processInstance);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual HistoricInstance GetHistoryProcessInstanceById(string processInstanceId)
        {
            IHistoricProcessInstance processInstance = (this.historyService as ServiceImpl).CommandExecutor.Execute(new Engine.Impl.Cmd.GetHistoricProcessInstanceByIdCmd(processInstanceId));

            return historicInstanceConverter.From(processInstance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstanceId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public IList<HistoricVariableInstance> GetHistoricVariables(string processInstanceId, string taskId)
        {
            return GetHistoricVariables(new ProcessVariablesQuery
            {
                ProcessInstanceId = processInstanceId,
                TaskId = taskId
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstanceId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public IList<HistoricVariableInstance> GetHistoricVariables(ProcessVariablesQuery qo)
        {
            var query = this.historyService.CreateHistoricVariableInstanceQuery()
                .SetProcessInstanceId(qo.ProcessInstanceId)
                .SetTaskId(qo.TaskId)
                .SetVariableName(qo.VariableName);

            if (qo.ExcludeTaskVariables)
            {
                query.SetExcludeTaskVariables();
            }

            IList<IHistoricVariableInstance> variableInstances = query.List();

            IList<HistoricVariableInstance> resourcesList = new List<HistoricVariableInstance>();
            foreach (IHistoricVariableInstance variableInstance in variableInstances)
            {
                resourcesList.Add(new HistoricVariableInstance(variableInstance.ProcessInstanceId, variableInstance.VariableName, variableInstance.VariableTypeName, variableInstance.Value, variableInstance.TaskId));
            }

            return resourcesList;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<string> GetActiveActivityIds(string executionId)
        {
            return runtimeService.GetActiveActivityIds(executionId);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<TaskModel> GetTasks(Pageable pageable)
        {
            return pageableTaskService.GetTasks(pageable);
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual IPage<TaskModel> GetAllTasks(Pageable pageable)
        {
            return pageableTaskService.GetAllTasks(pageable);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TaskModel GetTaskById(string taskId)
        {
            ITask task = (taskService as ServiceImpl).CommandExecutor.Execute(new Engine.Impl.Cmd.GetTaskByIdCmd(taskId));

            return taskConverter.From(task);
        }

        public virtual TaskModel[] ReassignTaskUsers(ReassignTaskUserCmd cmd)
        {
            ITask[] tasks = taskService.ReassignTaskUsers(cmd.Assignees);

            return taskConverter.From(tasks).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TaskModel ClaimTask(ClaimTaskCmd claimTaskCmd)
        {
            taskService.Claim(claimTaskCmd.TaskId, claimTaskCmd.Assignee);

            return taskConverter.From(taskService.CreateTaskQuery().SetTaskId(claimTaskCmd.TaskId).SingleResult());
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TaskModel ReleaseTask(ReleaseTaskCmd cmd)
        {
            ITask task = (taskService as ServiceImpl).CommandExecutor.Execute(new Engine.Impl.Cmd.AssigneeReleaseTaskCmd(cmd.TaskId, cmd.BusinessKey, cmd.Assignee, cmd.Reason));

            return taskConverter.From(taskService.CreateTaskQuery()
                .SetTaskId(task.Id)
                .SingleResult());
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void CompleteTask(CompleteTaskCmd cmd)
        {
            if (cmd != null)
            {
                IDictionary<string, object> variables = new Dictionary<string, object>(cmd.OutputVariables);
                if (cmd.RuntimeAssigneeUser != null && (cmd.RuntimeAssigneeUser.Users?.Length).GetValueOrDefault() > 0)
                {
                    variables.Add(BpmnXMLConstants.RUNTIME_ASSIGNEE_USER_VARIABLE_NAME, cmd.RuntimeAssigneeUser);
                }

                if (string.IsNullOrWhiteSpace(cmd.TaskId) == false)
                {
                    taskService.Complete(cmd.TaskId, variables, cmd.LocalScope);
                }
                else if (string.IsNullOrWhiteSpace(cmd.BusinessKey) == false && string.IsNullOrWhiteSpace(cmd.Assignee) == false)
                {
                    taskService.Complete(cmd.BusinessKey, cmd.Assignee, cmd.Comment, variables, cmd.LocalScope);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void CompleteApprovalTask(CompleteTaskCmd completeTaskCmd)
        {
            IDictionary<string, object> variables = new Dictionary<string, object>(completeTaskCmd.OutputVariables);

            variables.TryGetValue(WorkflowVariable.GLOBAL_APPROVALED_COMMENTS, out var comment);

            taskService.Complete(completeTaskCmd.TaskId, comment?.ToString(), variables, completeTaskCmd.LocalScope);
        }

        /// <summary>
        /// 
        /// </summary>

        public void SetTaskVariables(SetTaskVariablesCmd value)
        {
            taskService.SetVariables(value.TaskId, value.Variables);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetTaskVariablesLocal(SetTaskVariablesCmd value)
        {
            taskService.SetVariablesLocal(value.TaskId, value.Variables);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetProcessVariables(SetProcessVariablesCmd value)
        {
            ProcessInstance processInstance = GetProcessInstanceById(value.ProcessId);
            VerifyCanWriteToProcessInstance(processInstance.Id);
            runtimeService.SetVariables(value.ProcessId, value.Variables);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void RemoveProcessVariables(RemoveProcessVariablesCmd removeProcessVariablesCmd)
        {
            ProcessInstance processInstance = GetProcessInstanceById(removeProcessVariablesCmd.ProcessId);
            VerifyCanWriteToProcessInstance(processInstance.Id);
            runtimeService.RemoveVariables(removeProcessVariablesCmd.ProcessId, removeProcessVariablesCmd.VariableNames);
        }

        /// <summary>
        /// Delete task by id. </summary>
        /// <param name="taskId"> the task id to delete </param>
        public virtual void DeleteTask(string taskId)
        {
            DeleteTask(taskId, "Cancelled by " + authenticationWrapper.AuthenticatedUser.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void DeleteTask(string taskId, string reason, bool cascade = false)
        {
            TaskModel task = GetTaskById(taskId);
            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find task for the given id: " + taskId);
            }

            CheckWritePermissionsOnTask(task);

            taskService.DeleteTask(taskId, reason, cascade);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void CheckWritePermissionsOnTask(TaskModel task)
        {
            //TODO: to check the user write permissions on task
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TaskModel CreateNewTask(CreateTaskCmd createTaskCmd)
        {
            ITask task = taskService.CreateNewTask(createTaskCmd.Name, createTaskCmd.Description, createTaskCmd.DueDate, createTaskCmd.Priority, createTaskCmd.ParentTaskId, createTaskCmd.Assignee, securityService.User.TenantId);

            return taskConverter.From(task);
        }

        /// <summary>
        /// 追加任务处理人
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public virtual TaskModel[] AppendCountersign(AppendCountersignCmd cmd)
        {
            ITask[] tasks = taskService.AddCountersign(cmd.TaskId, cmd.Assignees, securityService.User.TenantId);

            return tasks.Select(x => taskConverter.From(x)).ToArray();
        }

        /// <summary>
        /// 终止任务
        /// </summary>
        /// <param name="cmd">终止任务命令</param>
        public virtual void TerminateTask(TerminateTaskCmd cmd)
        {
            taskService.TerminateTask(cmd.TaskId, cmd.TerminateReason, true);
        }

        /// <summary>
        /// 任务转办
        /// </summary>
        /// <param name="cmd">任务转办命令</param>
        /// <returns></returns>
        public virtual TaskModel[] TransferTask(ITransferTaskCmd cmd)
        {
            ITask[] tasks = taskService.Transfer(cmd);

            return tasks.Select(x => taskConverter.From(x)).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TaskModel CreateNewSubtask(CreateTaskCmd createSubtaskCmd)
        {
            ITask task = taskService.CreateNewSubtask(createSubtaskCmd.Name, createSubtaskCmd.Description, createSubtaskCmd.DueDate, createSubtaskCmd.Priority, createSubtaskCmd.ParentTaskId, createSubtaskCmd.Assignee, securityService.User.TenantId);

            return taskConverter.From(task);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void TerminateProcessInstance(string processInstanceId)
        {
            TerminateProcessInstance(processInstanceId, "Cancelled by " + authenticationWrapper.AuthenticatedUser.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void TerminateProcessInstance(string processInstanceId, string reason)
        {
            VerifyCanWriteToProcessInstance(processInstanceId);
            runtimeService.DeleteProcessInstance(processInstanceId, reason ?? "Cancelled");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<ITask> GetSubtasks(string parentTaskId)
        {
            return taskService.GetSubTasks(parentTaskId);
        }

        /// <summary>
        /// 更新任务
        /// </summary>
        public virtual TaskModel UpdateTask(IUpdateTaskCmd updateTaskCmd)
        {
            ITask task = taskService.UpdateTask(updateTaskCmd);

            return taskConverter.From(task);
        }
    }
}