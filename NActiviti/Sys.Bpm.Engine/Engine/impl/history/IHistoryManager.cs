using System;

namespace Sys.Workflow.engine.impl.history
{

    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// <summary>
    /// 
    /// </summary>
    public interface IHistoryManager
    {
        /// <returns> true, if the configured history-level is equal to OR set to a higher value than the given level. </returns>
        bool IsHistoryLevelAtLeast(HistoryLevel level);

        /// <returns> true, if history-level is configured to level other than "none". </returns>
        bool HistoryEnabled { get; }

        /// <summary>
        /// Record a process-instance ended. Updates the historic process instance if activity history is enabled.
        /// </summary>
        void RecordProcessInstanceEnd(string processInstanceId, string deleteReason, string activityId);

        /// <summary>
        /// Record a process-instance started and record start-event if activity history is enabled.
        /// </summary>
        void RecordProcessInstanceStart(IExecutionEntity processInstance, FlowElement startElement);

        /// <summary>
        /// Record a process-instance name change.
        /// </summary>
        void RecordProcessInstanceNameChange(string processInstanceId, string newName);

        /// <summary>
        /// Record a sub-process-instance started and alters the calledProcessinstanceId on the current active activity's historic counterpart. Only effective when activity history is enabled.
        /// </summary>
        void RecordSubProcessInstanceStart(IExecutionEntity parentExecution, IExecutionEntity subProcessInstance, FlowElement initialFlowElement);

        /// <summary>
        /// Record the start of an activity, if activity history is enabled.
        /// </summary>
        void RecordActivityStart(IExecutionEntity executionEntity);

        /// <summary>
        /// Record the end of an activity, if activity history is enabled.
        /// </summary>
        void RecordActivityEnd(IExecutionEntity executionEntity, string deleteReason);

        /// <summary>
        /// Finds the <seealso cref="IHistoricActivityInstanceEntity"/> that is active in the given execution.
        /// </summary>
        IHistoricActivityInstanceEntity FindActivityInstance(IExecutionEntity execution, bool createOnNotFound, bool validateEndTimeNull);

        /// <summary>
        /// Record a change of the process-definition id of a process instance, if activity history is enabled.
        /// </summary>
        void RecordProcessDefinitionChange(string processInstanceId, string processDefinitionId);

        /// <summary>
        /// Record the creation of a task, if audit history is enabled.
        /// </summary>
        void RecordTaskCreated(ITaskEntity task, IExecutionEntity execution);

        /// <summary>
        /// Record the assignment of task, if activity history is enabled.
        /// </summary>
        void RecordTaskAssignment(ITaskEntity task);

        /// <summary>
        /// record task instance claim time, if audit history is enabled
        /// </summary>
        /// <param name="task"> </param>

        void RecordTaskClaim(ITaskEntity task);

        /// <summary>
        /// Record the id of a the task associated with a historic activity, if activity history is enabled.
        /// </summary>
        void RecordTaskId(ITaskEntity task);

        /// <summary>
        /// Record task as ended, if audit history is enabled.
        /// </summary>
        void RecordTaskEnd(string taskId, string deleteReason);

        /// <summary>
        /// Record task assignee change, if audit history is enabled.
        /// </summary>
        void RecordTaskAssigneeChange(string taskId, string assignee, string assigneeUser);

        /// <summary>
        /// Record task owner change, if audit history is enabled.
        /// </summary>
        void RecordTaskOwnerChange(string taskId, string owner);

        /// <summary>
        /// Record task name change, if audit history is enabled.
        /// </summary>
        void RecordTaskNameChange(string taskId, string taskName);

        /// <summary>
        /// Record task description change, if audit history is enabled.
        /// </summary>
        void RecordTaskDescriptionChange(string taskId, string description);

        /// <summary>
        /// Record task due date change, if audit history is enabled.
        /// </summary>
        void RecordTaskDueDateChange(string taskId, DateTime dueDate);

        /// <summary>
        /// Record task priority change, if audit history is enabled.
        /// </summary>
        void RecordTaskPriorityChange(string taskId, int? priority);

        /// <summary>
        /// Record task category change, if audit history is enabled.
        /// </summary>
        void RecordTaskCategoryChange(string taskId, string category);

        /// <summary>
        /// Record task form key change, if audit history is enabled.
        /// </summary>
        void RecordTaskFormKeyChange(string taskId, string formKey);

        /// <summary>
        /// Record task parent task id change, if audit history is enabled.
        /// </summary>
        void RecordTaskParentTaskIdChange(string taskId, string parentTaskId);

        /// <summary>
        /// Record task execution id change, if audit history is enabled.
        /// </summary>
        void RecordTaskExecutionIdChange(string taskId, string executionId);

        /// <summary>
        /// Record task definition key change, if audit history is enabled.
        /// </summary>
        void RecordTaskDefinitionKeyChange(string taskId, string taskDefinitionKey);

        /// <summary>
        /// Record a change of the process-definition id of a task instance, if activity history is enabled.
        /// </summary>
        void RecordTaskProcessDefinitionChange(string taskId, string processDefinitionId);

        /// <summary>
        /// Record a variable has been created, if audit history is enabled.
        /// </summary>
        void RecordVariableCreate(IVariableInstanceEntity variable);

        /// <summary>
        /// Record a variable has been created, if audit history is enabled.
        /// </summary>
        void RecordHistoricDetailVariableCreate(IVariableInstanceEntity variable, IExecutionEntity sourceActivityExecution, bool useActivityId);

        /// <summary>
        /// Record a variable has been updated, if audit history is enabled.
        /// </summary>
        void RecordVariableUpdate(IVariableInstanceEntity variable);

        /// <summary>
        /// Record a variable has been deleted, if audit history is enabled.
        /// </summary>
        void RecordVariableRemoved(IVariableInstanceEntity variable);

        /// <summary>
        /// Creates a new comment to indicate a new <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void CreateIdentityLinkComment(string taskId, string userId, string groupId, string type, bool create);

        /// <summary>
        /// Creates a new comment to indicate a new user <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void CreateUserIdentityLinkComment(string taskId, string userId, string type, bool create);

        /// <summary>
        /// Creates a new comment to indicate a new group <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void CreateGroupIdentityLinkComment(string taskId, string groupId, string type, bool create);

        /// <summary>
        /// Creates a new comment to indicate a new <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void CreateIdentityLinkComment(string taskId, string userId, string groupId, string type, bool create, bool forceNullUserId);

        /// <summary>
        /// Creates a new comment to indicate a new user <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void CreateUserIdentityLinkComment(string taskId, string userId, string type, bool create, bool forceNullUserId);

        /// <summary>
        /// Creates a new comment to indicate a new <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void CreateProcessInstanceIdentityLinkComment(string processInstanceId, string userId, string groupId, string type, bool create);

        /// <summary>
        /// Creates a new comment to indicate a new <seealso cref="IdentityLink"/> has been created or deleted, if history is enabled.
        /// </summary>
        void CreateProcessInstanceIdentityLinkComment(string processInstanceId, string userId, string groupId, string type, bool create, bool forceNullUserId);

        /// <summary>
        /// Creates a new comment to indicate a new attachment has been created or deleted, if history is enabled.
        /// </summary>
        void CreateAttachmentComment(string taskId, string processInstanceId, string attachmentName, bool create);

        // Identity link related history
        /// <summary>
        /// Record the creation of a new <seealso cref="IdentityLink"/>, if audit history is enabled.
        /// </summary>
        void RecordIdentityLinkCreated(IIdentityLinkEntity identityLink);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        void DeleteHistoricIdentityLink(string id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstance"></param>
        void UpdateProcessBusinessKeyInHistory(IExecutionEntity processInstance);
    }
}