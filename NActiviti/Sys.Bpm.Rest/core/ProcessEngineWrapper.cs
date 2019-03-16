using Microsoft.Extensions.Logging;
using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.events.listeners;
using org.activiti.engine;
using org.activiti.engine.history;
using org.activiti.engine.impl.persistence.entity;
using org.activiti.engine.repository;
using org.activiti.engine.runtime;
using org.activiti.engine.task;
using org.springframework.context;
using Sys;
using System.Collections.Generic;
using System.Linq;

namespace org.activiti.cloud.services.core
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessEngineWrapper
    {
        private static readonly ILogger LOGGER = ProcessEngineServiceProvider.LoggerService<ProcessEngineWrapper>();

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
            HistoricInstanceConverter historicInstanceConverter)
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
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<ProcessInstance> getProcessInstances(Pageable pageable)
        {
            return pageableProcessInstanceService.getProcessInstances(pageable);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<ProcessInstance> getAllProcessInstances(Pageable pageable)
        {
            return pageableProcessInstanceService.getAllProcessInstances(pageable);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessInstance startProcess(StartProcessInstanceCmd cmd)
        {
            string processDefinitionKey = cmd.ProcessDefinitionKey;
            string id = cmd.ProcessDefinitionId;
            IProcessDefinition definition = null;
            if (string.IsNullOrWhiteSpace(cmd.StartForm) == false)
            {
                definition = repositoryService.createProcessDefinitionQuery()
                    .processDefinitionStartForm(cmd.StartForm)
                    .processDefinitionTenantId(cmd.TenantId)
                    .latestVersion()
                    .singleResult();

                if (definition == null)
                {
                    throw new ActivitiObjectNotFoundException("Unable to find process definition for the given form key:'" + cmd.StartForm + "'");
                }
                id = definition.Id;
            }
            else if (string.IsNullOrWhiteSpace(processDefinitionKey) == false)
            {
                definition = repositoryService.createProcessDefinitionQuery()
                    .processDefinitionKey(processDefinitionKey)
                    .processDefinitionTenantId(cmd.TenantId)
                    .latestVersion()
                    .singleResult();

                if (definition == null)
                {
                    throw new ActivitiObjectNotFoundException("Unable to find process definition for the given key:'" + processDefinitionKey + "'");
                }
                id = definition.Id;
            }
            else
            {
                definition = repositoryService.getProcessDefinition(id);
                if (definition == null)
                {
                    throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + id + "'");
                }
                id = definition.Id;
            }

            //if (!securityService.canWrite(processDefinitionKey))
            //{
            //    LOGGER.debug("User " + authenticationWrapper.AuthenticatedUserId + " not permitted to access definition " + processDefinitionKey);
            //    throw new ActivitiForbiddenException("Operation not permitted for " + processDefinitionKey);
            //}

            IProcessInstanceBuilder builder = runtimeService.createProcessInstanceBuilder();
            builder.processDefinitionId(definition.Id);
            if (string.IsNullOrWhiteSpace(cmd.ProcessInstanceName))
            {
                cmd.ProcessInstanceName = definition.Name;
            }
            builder.variables(cmd.Variables);
            builder.businessKey(cmd.BusinessKey);
            builder.name(cmd.ProcessInstanceName);
            builder.tenantId(cmd.TenantId);

            return processInstanceConverter.from(builder.start());
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void signal(SignalCmd signalCmd)
        {
            //TODO: plan is to restrict access to events using a new security policy on events
            // - that's another piece of work though so for now no security here

            runtimeService.signalEventReceived(signalCmd.Name, signalCmd.InputVariables);
            eventPublisher.publishEvent(signalCmd);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void suspend(SuspendProcessInstanceCmd suspendProcessInstanceCmd)
        {
            verifyCanWriteToProcessInstance(suspendProcessInstanceCmd.ProcessInstanceId);
            runtimeService.suspendProcessInstanceById(suspendProcessInstanceCmd.ProcessInstanceId);
        }

        private void verifyCanWriteToProcessInstance(string processInstanceId)
        {
            ProcessInstance processInstance = getProcessInstanceById(processInstanceId);
            if (processInstance == null)
            {
                throw new ActivitiException("Unable to find process instance for the given id: " + processInstanceId);
            }

            IProcessDefinition processDefinition = repositoryService.getProcessDefinition(processInstance.ProcessDefinitionId);
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
        public virtual void activate(ActivateProcessInstanceCmd activateProcessInstanceCmd)
        {
            verifyCanWriteToProcessInstance(activateProcessInstanceCmd.ProcessInstanceId);
            runtimeService.activateProcessInstanceById(activateProcessInstanceCmd.ProcessInstanceId);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessInstance getProcessInstanceById(string processInstanceId)
        {
            IProcessInstanceQuery query = runtimeService.createProcessInstanceQuery();
            query = query.processInstanceId(processInstanceId);
            IProcessInstance processInstance = query.singleResult();
            return processInstanceConverter.from(processInstance);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual HistoricInstance getHistoryProcessInstanceById(string processInstanceId)
        {
            IHistoricProcessInstanceQuery query = this.historyService.createHistoricProcessInstanceQuery().processInstanceId(processInstanceId);

            IHistoricProcessInstance processInstance = query.singleResult();

            return historicInstanceConverter.from(processInstance);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<string> getActiveActivityIds(string executionId)
        {
            return runtimeService.getActiveActivityIds(executionId);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<TaskModel> getTasks(Pageable pageable)
        {
            return pageableTaskService.getTasks(pageable);
        }
        /// <summary>
        /// 
        /// </summary>

        public virtual IPage<TaskModel> getAllTasks(Pageable pageable)
        {
            return pageableTaskService.getAllTasks(pageable);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TaskModel getTaskById(string taskId)
        {
            ITask task = taskService.createTaskQuery().taskId(taskId).singleResult();
            return taskConverter.from(task);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TaskModel claimTask(ClaimTaskCmd claimTaskCmd)
        {
            taskService.claim(claimTaskCmd.TaskId, claimTaskCmd.Assignee);
            return taskConverter.from(taskService.createTaskQuery().taskId(claimTaskCmd.TaskId).singleResult());
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TaskModel releaseTask(ReleaseTaskCmd releaseTaskCmd)
        {
            taskService.unclaim(releaseTaskCmd.TaskId);
            return taskConverter.from(taskService.createTaskQuery().taskId(releaseTaskCmd.TaskId).singleResult());
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void completeTask(CompleteTaskCmd completeTaskCmd)
        {
            if (completeTaskCmd != null)
            {
                taskService.complete(completeTaskCmd.TaskId, completeTaskCmd.OutputVariables);
            }
        }
        /// <summary>
        /// 
        /// </summary>

        public virtual SetTaskVariablesCmd TaskVariables
        {
            set
            {
                taskService.setVariables(value.TaskId, value.Variables);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual SetTaskVariablesCmd TaskVariablesLocal
        {
            set
            {
                taskService.setVariablesLocal(value.TaskId, value.Variables);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual SetProcessVariablesCmd ProcessVariables
        {
            set
            {
                ProcessInstance processInstance = getProcessInstanceById(value.ProcessId);
                verifyCanWriteToProcessInstance(processInstance.Id);
                runtimeService.setVariables(value.ProcessId, value.Variables);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void removeProcessVariables(RemoveProcessVariablesCmd removeProcessVariablesCmd)
        {
            ProcessInstance processInstance = getProcessInstanceById(removeProcessVariablesCmd.ProcessId);
            verifyCanWriteToProcessInstance(processInstance.Id);
            runtimeService.removeVariables(removeProcessVariablesCmd.ProcessId, removeProcessVariablesCmd.VariableNames);
        }

        /// <summary>
        /// Delete task by id. </summary>
        /// <param name="taskId"> the task id to delete </param>
        public virtual void deleteTask(string taskId)
        {
            deleteTask(taskId, "Cancelled by " + authenticationWrapper.AuthenticatedUser.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void deleteTask(string taskId, string reason)
        {
            TaskModel task = getTaskById(taskId);
            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find task for the given id: " + taskId);
            }

            checkWritePermissionsOnTask(task);

            taskService.deleteTask(taskId, reason ?? "Cancelled");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void checkWritePermissionsOnTask(TaskModel task)
        {
            //TODO: to check the user write permissions on task
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TaskModel createNewTask(CreateTaskCmd createTaskCmd)
        {
            ITask task = taskService.newTask();
            task.Name = createTaskCmd.Name;
            task.Description = createTaskCmd.Description;
            task.DueDate = createTaskCmd.DueDate;
            if (createTaskCmd.Priority != null)
            {
                task.Priority = createTaskCmd.Priority;
            }

            task.Assignee = string.ReferenceEquals(createTaskCmd.Assignee, null) ? authenticationWrapper.AuthenticatedUser.Id : createTaskCmd.Assignee;
            taskService.saveTask(task);

            return taskConverter.from(taskService.createTaskQuery().taskId(task.Id).singleResult());
        }

        /// <summary>
        /// 追加任务处理人
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public virtual TaskModel[] appendCountersign(AppendCountersignCmd cmd)
        {
            ITask task = taskService.createTaskQuery().taskId(cmd.TaskId).singleResult();

            var acmd = new engine.impl.cmd.AddCountersignCmd(task.ExecutionId, cmd.Assignees);

            ITask[] tasks = processEngine.ProcessEngineConfiguration.ManagementService.executeCommand(acmd) as ITask[];

            return tasks.Select(x => taskConverter.from(x)).ToArray();
        }

        /// <summary>
        /// 终止任务
        /// </summary>
        /// <param name="cmd">终止任务命令</param>
        public virtual void terminateTask(TerminateTaskCmd cmd)
        {
            taskService.terminateTask(cmd.TaskId, cmd.TerminateReason, true);
        }

        /// <summary>
        /// 任务转办
        /// </summary>
        /// <param name="cmd">任务转办命令</param>
        /// <returns></returns>
        public virtual TaskModel[] transferTask(TransferTaskCmd cmd)
        {
            ITaskEntity task = taskService.createTaskQuery().taskId(cmd.TaskId).singleResult() as ITaskEntity;
            if (task == null)
            {
                throw new ActivitiException("Parent task with id " + cmd.TaskId + " was not found");
            }

            taskService.terminateTask(task.Id, string.IsNullOrWhiteSpace(cmd.Description) ? "任务已转派" : cmd.Description, false);
            IList<TaskModel> tasks = new List<TaskModel>();
            foreach (var assignee in cmd.Assignees)
            {
                ITaskEntity newTask = taskService.newTask() as ITaskEntity;
                newTask.Name = string.IsNullOrWhiteSpace(cmd.Name) ? task.Name : cmd.Name;
                newTask.Category = task.Category;
                newTask.FormKey = task.FormKey;
                newTask.ProcessDefinitionId = task.ProcessDefinitionId;
                newTask.ProcessInstanceId = task.ProcessInstanceId;
                newTask.TenantId = task.TenantId;
                //不要设置executionid
                newTask.ExecutionId = task.ExecutionId;
                newTask.Description = cmd.Description;
                newTask.TaskDefinitionKey = task.TaskDefinitionKey;
                newTask.DueDate = cmd.DueDate;
                if (cmd.Priority != null)
                {
                    newTask.Priority = cmd.Priority;
                }

                newTask.Assignee = string.ReferenceEquals(assignee, null) ?
                    authenticationWrapper.AuthenticatedUser.Id : assignee;

                taskService.saveTask(newTask);

                tasks.Add(taskConverter.from(taskService.createTaskQuery().taskId(newTask.Id).singleResult()));
            }

            return tasks.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TaskModel createNewSubtask(string parentTaskId, CreateTaskCmd createSubtaskCmd)
        {
            if (taskService.createTaskQuery().taskId(parentTaskId).singleResult() == null)
            {
                throw new ActivitiException("Parent task with id " + parentTaskId + " was not found");
            }

            ITask task = taskService.newTask();
            task.Name = createSubtaskCmd.Name;
            task.Description = createSubtaskCmd.Description;
            task.DueDate = createSubtaskCmd.DueDate;
            if (createSubtaskCmd.Priority != null)
            {
                task.Priority = createSubtaskCmd.Priority;
            }
            task.ParentTaskId = parentTaskId;

            task.Assignee = string.ReferenceEquals(createSubtaskCmd.Assignee, null) ? authenticationWrapper.AuthenticatedUser.Id : createSubtaskCmd.Assignee;
            taskService.saveTask(task);

            return taskConverter.from(taskService.createTaskQuery().taskId(task.Id).singleResult());
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void deleteProcessInstance(string processInstanceId)
        {
            deleteProcessInstance(processInstanceId, "Cancelled by " + authenticationWrapper.AuthenticatedUser.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void deleteProcessInstance(string processInstanceId, string reason)
        {
            verifyCanWriteToProcessInstance(processInstanceId);
            runtimeService.deleteProcessInstance(processInstanceId, reason ?? "Cancelled");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<ITask> getSubtasks(string parentTaskId)
        {
            return taskService.getSubTasks(parentTaskId);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void updateTask(string taskId, UpdateTaskCmd updateTaskCmd)
        {
            ITask task = taskService.createTaskQuery().taskId(taskId).singleResult();
            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find task for the given id: " + updateTaskCmd.Id);
            }

            if (!string.ReferenceEquals(updateTaskCmd.Assignee, null))
            {
                task.Assignee = updateTaskCmd.Assignee;
            }
            if (!string.ReferenceEquals(updateTaskCmd.Name, null))
            {
                task.Name = updateTaskCmd.Name;
            }
            if (!string.ReferenceEquals(updateTaskCmd.Description, null))
            {
                task.Description = updateTaskCmd.Description;
            }
            if (updateTaskCmd.DueDate != null)
            {
                task.DueDate = updateTaskCmd.DueDate;
            }
            if (updateTaskCmd.Priority != null)
            {
                task.Priority = updateTaskCmd.Priority;
            }
            if (!string.ReferenceEquals(updateTaskCmd.ParentTaskId, null))
            {
                task.ParentTaskId = updateTaskCmd.ParentTaskId;
            }
            taskService.saveTask(task);
        }
    }

}