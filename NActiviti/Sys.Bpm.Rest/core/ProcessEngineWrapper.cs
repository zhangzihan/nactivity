using Microsoft.Extensions.Logging;
using org.activiti.api.runtime.shared.query;
using org.activiti.bpmn.constants;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.events.listeners;
using org.activiti.engine;
using org.activiti.engine.history;
using org.activiti.engine.impl;
using org.activiti.engine.impl.persistence.entity;
using org.activiti.engine.repository;
using org.activiti.engine.runtime;
using org.activiti.engine.task;
using org.activiti.services.api.commands;
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
        public virtual ProcessInstance startProcess(IStartProcessInstanceCmd cmd)
        {
            //if (!securityService.canWrite(processDefinitionKey))
            //{
            //    LOGGER.debug("User " + authenticationWrapper.AuthenticatedUserId + " not permitted to access definition " + processDefinitionKey);
            //    throw new ActivitiForbiddenException("Operation not permitted for " + processDefinitionKey);
            //}

            IProcessInstance processInstance = runtimeService.startProcessInstanceByCmd(cmd);

            return processInstanceConverter.from(processInstance);
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
                IDictionary<string, object> variables = new Dictionary<string, object>(completeTaskCmd.OutputVariables);
                if (completeTaskCmd.RuntimeAssigneeUser != null && completeTaskCmd.RuntimeAssigneeUser.Users?.Length > 0)
                {
                    variables.Add(BpmnXMLConstants.RUNTIME_ASSIGNEE_USER_VARIABLE_NAME, completeTaskCmd.RuntimeAssigneeUser);
                }
                taskService.complete(completeTaskCmd.TaskId, variables);
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
        public virtual void deleteTask(string taskId, string reason, bool cascade = false)
        {
            TaskModel task = getTaskById(taskId);
            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find task for the given id: " + taskId);
            }

            checkWritePermissionsOnTask(task);

            taskService.deleteTask(taskId, reason, cascade);
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
            ITask task = taskService.createNewTask(createTaskCmd.Name, createTaskCmd.Description, createTaskCmd.DueDate, createTaskCmd.Priority, createTaskCmd.ParentTaskId, createTaskCmd.Assignee, securityService.User.TenantId);

            return taskConverter.from(task);
        }

        /// <summary>
        /// 追加任务处理人
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public virtual TaskModel[] appendCountersign(AppendCountersignCmd cmd)
        {
            ITask[] tasks = taskService.addCountersign(cmd.TaskId, cmd.Assignees, securityService.User.TenantId);

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
        public virtual TaskModel[] transferTask(ITransferTaskCmd cmd)
        {
            ITask[] tasks = taskService.transfer(cmd);

            return tasks.Select(x => taskConverter.from(x)).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TaskModel createNewSubtask(string parentTaskId, CreateTaskCmd createSubtaskCmd)
        {
            ITask task = taskService.createNewSubtask(createSubtaskCmd.Name, createSubtaskCmd.Description, createSubtaskCmd.DueDate, createSubtaskCmd.Priority, parentTaskId, createSubtaskCmd.Assignee, securityService.User.TenantId);

            return taskConverter.from(task);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void terminateProcessInstance(string processInstanceId)
        {
            terminateProcessInstance(processInstanceId, "Cancelled by " + authenticationWrapper.AuthenticatedUser.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void terminateProcessInstance(string processInstanceId, string reason)
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
        /// 更新任务
        /// </summary>
        public virtual TaskModel updateTask(string taskId, IUpdateTaskCmd updateTaskCmd)
        {
            ITask task = taskService.updateTask(updateTaskCmd);

            return taskConverter.from(task);
        }
    }
}