using System;

namespace org.activiti.engine.impl.history
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.persistence.entity;

    public interface IHistoryManager
    {

        /// <returns> true, if the configured history-level is equal to OR set to a higher value than the given level. </returns>
        bool isHistoryLevelAtLeast(HistoryLevel level);

        /// <returns> true, if history-level is configured to level other than "none". </returns>
        bool HistoryEnabled { get; }

        /// <summary>
        /// Record a process-instance ended. Updates the historic process instance if activity history is enabled.
        /// </summary>
        void recordProcessInstanceEnd(string processInstanceId, string deleteReason, string activityId);

        /// <summary>
        /// Record a process-instance started and record start-event if activity history is enabled.
        /// </summary>
        void recordProcessInstanceStart(IExecutionEntity processInstance, FlowElement startElement);

        /// <summary>
        /// Record a process-instance name change.
        /// </summary>
        void recordProcessInstanceNameChange(string processInstanceId, string newName);

        /// <summary>
        /// Record a sub-process-instance started and alters the calledProcessinstanceId on the current active activity's historic counterpart. Only effective when activity history is enabled.
        /// </summary>
        void recordSubProcessInstanceStart(IExecutionEntity parentExecution, IExecutionEntity subProcessInstance, FlowElement initialFlowElement);

        /// <summary>
        /// Record the start of an activity, if activity history is enabled.
        /// </summary>
        void recordActivityStart(IExecutionEntity executionEntity);

        /// <summary>
        /// Record the end of an activity, if activity history is enabled.
        /// </summary>
        void recordActivityEnd(IExecutionEntity executionEntity, string deleteReason);

        /// <summary>
        /// Finds the <seealso cref="IHistoricActivityInstanceEntity"/> that is active in the given execution.
        /// </summary>
        IHistoricActivityInstanceEntity findActivityInstance(IExecutionEntity execution, bool createOnNotFound, bool validateEndTimeNull);

        /// <summary>
        /// Record a change of the process-definition id of a process instance, if activity history is enabled.
        /// </summary>
        void recordProcessDefinitionChange(string processInstanceId, string processDefinitionId);

        /// <summary>
        /// Record the creation of a task, if audit history is enabled.
        /// </summary>
        void recordTaskCreated(ITaskEntity task, IExecutionEntity execution);

        /// <summary>
        /// Record the assignment of task, if activity history is enabled.
        /// </summary>
        void recordTaskAssignment(ITaskEntity task);

        /// <summary>
        /// record task instance claim time, if audit history is enabled
        /// </summary>
        /// <param name="task"> </param>

        void recordTaskClaim(ITaskEntity task);

        /// <summary>
        /// Record the id of a the task associated with a historic activity, if activity history is enabled.
        /// </summary>
        void recordTaskId(ITaskEntity task);

        /// <summary>
        /// Record task as ended, if audit history is enabled.
        /// </summary>
        void recordTaskEnd(string taskId, string deleteReason);

        /// <summary>
        /// Record task assignee change, if audit history is enabled.
        /// </summary>
        void recordTaskAssigneeChange(string taskId, string assignee);

        /// <summary>
        /// Record task owner change, if audit history is enabled.
        /// </summary>
        void recordTaskOwnerChange(string taskId, string owner);

        /// <summary>
        /// Record task name change, if audit history is enabled.
        /// </summary>
        void recordTaskNameChange(string taskId, string taskName);

        /// <summary>
        /// Record task description change, if audit history is enabled.
        /// </summary>
        void recordTaskDescriptionChange(string taskId, string description);

        /// <summary>
        /// Record task due date change, if audit history is enabled.
        /// </summary>
        void recordTaskDueDateChange(string taskId, DateTime dueDate);

        /// <summary>
        /// Record task priority change, if audit history is enabled.
        /// </summary>
        void recordTaskPriorityChange(string taskId, int? priority);

        /// <summary>
        /// Record task category change, if audit history is enabled.
        /// </summary>
        void recordTaskCategoryChange(string taskId, string category);

        /// <summary>
        /// Record task form key change, if audit history is enabled.
        /// </summary>
        void recordTaskFormKeyChange(string taskId, string formKey);

        /// <summary>
        /// Record task parent task id change, if audit history is enabled.
        /// </summary>
        void recordTaskParentTaskIdChange(string taskId, string parentTaskId);

        /// <summary>
        /// Record task execution id change, if audit history is enabled.
        /// </summary>
        void recordTaskExecutionIdChange(string taskId, string executionId);

        /// <summary>
        /// Record task definition key change, if audit history is enabled.
        /// </summary>
        void recordTaskDefinitionKeyChange(string taskId, string taskDefinitionKey);

        /// <summary>
        /// Record a change of the process-definition id of a task instance, if activity history is enabled.
        /// </summary>
        void recordTaskProcessDefinitionChange(string taskId, string processDefinitionId);

        /// <summary>
        /// Record a variable has been created, if audit history is enabled.
        /// </summary>
        void recordVariableCreate(IVariableInstanceEntity variable);

        /// <summary>
        /// Record a variable has been created, if audit history is enabled.
        /// </summary>
        void recordHistoricDetailVariableCreate(IVariableInstanceEntity variable, IExecutionEntity sourceActivityExecution, bool useActivityId);

        /// <summary>
        /// Record a variable has been updated, if audit history is enabled.
        /// </summary>
        void recordVariableUpdate(IVariableInstanceEntity variable);

        /// <summary>
        /// Record a variable has been deleted, if audit history is enabled.
        /// </summary>
        void recordVariableRemoved(IVariableInstanceEntity variable);

        /// <summary>
        /// Creates a new comment to indicate a new <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void createIdentityLinkComment(string taskId, string userId, string groupId, string type, bool create);

        /// <summary>
        /// Creates a new comment to indicate a new user <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void createUserIdentityLinkComment(string taskId, string userId, string type, bool create);

        /// <summary>
        /// Creates a new comment to indicate a new group <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void createGroupIdentityLinkComment(string taskId, string groupId, string type, bool create);

        /// <summary>
        /// Creates a new comment to indicate a new <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void createIdentityLinkComment(string taskId, string userId, string groupId, string type, bool create, bool forceNullUserId);

        /// <summary>
        /// Creates a new comment to indicate a new user <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void createUserIdentityLinkComment(string taskId, string userId, string type, bool create, bool forceNullUserId);

        /// <summary>
        /// Creates a new comment to indicate a new <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void createProcessInstanceIdentityLinkComment(string processInstanceId, string userId, string groupId, string type, bool create);

        /// <summary>
        /// Creates a new comment to indicate a new <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void createProcessInstanceIdentityLinkComment(string processInstanceId, string userId, string groupId, string type, bool create, bool forceNullUserId);

        /// <summary>
        /// Creates a new comment to indicate a new attachment has been created or deleted, if history is enabled.
        /// </summary>
        void createAttachmentComment(string taskId, string processInstanceId, string attachmentName, bool create);

        // Identity link related history
        /// <summary>
        /// Record the creation of a new <seealso cref="IdentityLink"/>, if audit history is enabled.
        /// </summary>
        void recordIdentityLinkCreated(IIdentityLinkEntity identityLink);

        void deleteHistoricIdentityLink(string id);

        void updateProcessBusinessKeyInHistory(IExecutionEntity processInstance);

    }

}