using Microsoft.Extensions.Logging;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.events.listeners;
using org.activiti.cloud.services.rest.controllers;
using org.activiti.engine;
using org.activiti.engine.repository;
using org.activiti.engine.runtime;
using org.activiti.engine.task;
using org.springframework.context;
using org.springframework.data.domain;
using Sys;
using System.Collections.Generic;

namespace org.activiti.cloud.services.core
{
    public class ProcessEngineWrapper
    {
        private static readonly ILogger LOGGER = ProcessEngineServiceProvider.LoggerService<ProcessEngineWrapper>();

        private readonly ProcessInstanceConverter processInstanceConverter;
        private readonly IRuntimeService runtimeService;
        private readonly ITaskService taskService;
        private readonly TaskConverter taskConverter;
        private readonly PageableTaskService pageableTaskService;
        private readonly SecurityPoliciesApplicationService securityService;
        private readonly IRepositoryService repositoryService;
        private readonly AuthenticationWrapper authenticationWrapper;
        private PageableProcessInstanceService pageableProcessInstanceService;
        private readonly ApplicationEventPublisher eventPublisher;

        public ProcessEngineWrapper(ProcessInstanceConverter processInstanceConverter, 
            IRuntimeService runtimeService, 
            PageableProcessInstanceService pageableProcessInstanceService, 
            ITaskService taskService, 
            TaskConverter taskConverter, 
            PageableTaskService pageableTaskService, 
            MessageProducerActivitiEventListener listener, 
            SecurityPoliciesApplicationService securityService, 
            IRepositoryService repositoryService, 
            AuthenticationWrapper authenticationWrapper, 
            ApplicationEventPublisher eventPublisher)
        {
            this.processInstanceConverter = processInstanceConverter;
            this.runtimeService = runtimeService;
            this.pageableProcessInstanceService = pageableProcessInstanceService;
            this.taskService = taskService;
            this.taskConverter = taskConverter;
            this.pageableTaskService = pageableTaskService;
#warning 暂时不处理事件侦听
            //this.runtimeService.addEventListener(listener);
            this.securityService = securityService;
            this.repositoryService = repositoryService;
            this.authenticationWrapper = authenticationWrapper;
            this.eventPublisher = eventPublisher;
        }

        public virtual Page<ProcessInstance> getProcessInstances(Pageable pageable)
        {
            return pageableProcessInstanceService.getProcessInstances(pageable);
        }

        public virtual Page<ProcessInstance> getAllProcessInstances(Pageable pageable)
        {
            return pageableProcessInstanceService.getAllProcessInstances(pageable);
        }

        public virtual ProcessInstance startProcess(StartProcessInstanceCmd cmd)
        {

            string processDefinitionKey = null;
            if (!string.ReferenceEquals(cmd.ProcessDefinitionKey, null))
            {
                long count = repositoryService.createProcessDefinitionQuery().processDefinitionKey(cmd.ProcessDefinitionKey).count();
                if (count == 0)
                {
                    throw new ActivitiObjectNotFoundException("Unable to find process definition for the given key:'" + cmd.ProcessDefinitionKey + "'");
                }
                processDefinitionKey = cmd.ProcessDefinitionKey;
            }
            else
            {
                IProcessDefinition definition = repositoryService.getProcessDefinition(cmd.ProcessDefinitionId);
                if (definition == null)
                {
                    throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + cmd.ProcessDefinitionId + "'");
                }
                processDefinitionKey = definition.Key;
            }

            //if (!securityService.canWrite(processDefinitionKey))
            //{
            //    LOGGER.debug("User " + authenticationWrapper.AuthenticatedUserId + " not permitted to access definition " + processDefinitionKey);
            //    throw new ActivitiForbiddenException("Operation not permitted for " + processDefinitionKey);
            //}

            IProcessInstanceBuilder builder = runtimeService.createProcessInstanceBuilder();
            if (!string.ReferenceEquals(cmd.ProcessDefinitionKey, null))
            {
                builder.processDefinitionKey(cmd.ProcessDefinitionKey);
            }
            else
            {
                builder.processDefinitionId(cmd.ProcessDefinitionId);
            }
            builder.variables(cmd.Variables);
            builder.businessKey(cmd.BusinessKey);
            return processInstanceConverter.from(builder.start());
        }

        public virtual void signal(SignalCmd signalCmd)
        {
            //TODO: plan is to restrict access to events using a new security policy on events
            // - that's another piece of work though so for now no security here

            runtimeService.signalEventReceived(signalCmd.Name, signalCmd.InputVariables);
            eventPublisher.publishEvent(signalCmd);
        }

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

        public virtual void activate(ActivateProcessInstanceCmd activateProcessInstanceCmd)
        {
            verifyCanWriteToProcessInstance(activateProcessInstanceCmd.ProcessInstanceId);
            runtimeService.activateProcessInstanceById(activateProcessInstanceCmd.ProcessInstanceId);
        }

        public virtual ProcessInstance getProcessInstanceById(string processInstanceId)
        {
            IProcessInstanceQuery query = runtimeService.createProcessInstanceQuery();
            query = query.processInstanceId(processInstanceId);
            IProcessInstance processInstance = query.singleResult();
            return processInstanceConverter.from(processInstance);
        }

        public virtual IList<string> getActiveActivityIds(string executionId)
        {
            return runtimeService.getActiveActivityIds(executionId);
        }

        public virtual Page<Task> getTasks(Pageable pageable)
        {
            return pageableTaskService.getTasks(pageable);
        }

        public virtual Page<Task> getAllTasks(Pageable pageable)
        {
            return pageableTaskService.getAllTasks(pageable);
        }

        public virtual Task getTaskById(string taskId)
        {
            ITask task = taskService.createTaskQuery().taskId(taskId).singleResult();
            return taskConverter.from(task);
        }

        public virtual Task claimTask(ClaimTaskCmd claimTaskCmd)
        {
            taskService.claim(claimTaskCmd.TaskId, claimTaskCmd.Assignee);
            return taskConverter.from(taskService.createTaskQuery().taskId(claimTaskCmd.TaskId).singleResult());
        }

        public virtual Task releaseTask(ReleaseTaskCmd releaseTaskCmd)
        {
            taskService.unclaim(releaseTaskCmd.TaskId);
            return taskConverter.from(taskService.createTaskQuery().taskId(releaseTaskCmd.TaskId).singleResult());
        }

        public virtual void completeTask(CompleteTaskCmd completeTaskCmd)
        {
            if (completeTaskCmd != null)
            {
                taskService.complete(completeTaskCmd.TaskId, completeTaskCmd.OutputVariables);
            }
        }

        public virtual SetTaskVariablesCmd TaskVariables
        {
            set
            {
                taskService.setVariables(value.TaskId, value.Variables);
            }
        }

        public virtual SetTaskVariablesCmd TaskVariablesLocal
        {
            set
            {
                taskService.setVariablesLocal(value.TaskId, value.Variables);
            }
        }

        public virtual SetProcessVariablesCmd ProcessVariables
        {
            set
            {
                ProcessInstance processInstance = getProcessInstanceById(value.ProcessId);
                verifyCanWriteToProcessInstance(processInstance.Id);
                runtimeService.setVariables(value.ProcessId, value.Variables);
            }
        }

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
            Task task = getTaskById(taskId);
            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find task for the given id: " + taskId);
            }

            checkWritePermissionsOnTask(task);

            taskService.deleteTask(taskId, "Cancelled by " + authenticationWrapper.AuthenticatedUserId);
        }

        public virtual void checkWritePermissionsOnTask(Task task)
        {
            //TODO: to check the user write permissions on task
        }

        public virtual Task createNewTask(CreateTaskCmd createTaskCmd)
        {
            ITask task = taskService.newTask();
            task.Name = createTaskCmd.Name;
            task.Description = createTaskCmd.Description;
            task.DueDate = createTaskCmd.DueDate;
            if (createTaskCmd.Priority != null)
            {
                task.Priority = createTaskCmd.Priority;
            }

            task.Assignee = string.ReferenceEquals(createTaskCmd.Assignee, null) ? authenticationWrapper.AuthenticatedUserId : createTaskCmd.Assignee;
            taskService.saveTask(task);

            return taskConverter.from(taskService.createTaskQuery().taskId(task.Id).singleResult());
        }

        public virtual Task createNewSubtask(string parentTaskId, CreateTaskCmd createSubtaskCmd)
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

            task.Assignee = string.ReferenceEquals(createSubtaskCmd.Assignee, null) ? authenticationWrapper.AuthenticatedUserId : createSubtaskCmd.Assignee;
            taskService.saveTask(task);

            return taskConverter.from(taskService.createTaskQuery().taskId(task.Id).singleResult());
        }

        public virtual void deleteProcessInstance(string processInstanceId)
        {
            verifyCanWriteToProcessInstance(processInstanceId);
            runtimeService.deleteProcessInstance(processInstanceId, "Cancelled by " + authenticationWrapper.AuthenticatedUserId);
        }

        public virtual IList<ITask> getSubtasks(string parentTaskId)
        {
            return taskService.getSubTasks(parentTaskId);
        }

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