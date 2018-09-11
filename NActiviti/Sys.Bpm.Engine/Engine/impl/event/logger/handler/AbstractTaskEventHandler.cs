using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public abstract class AbstractTaskEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        protected internal virtual IDictionary<string, object> handleCommonTaskFields(ITaskEntity task)
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
            putInMapIfNotNull(data, Fields_Fields.ID, task.Id);
            putInMapIfNotNull(data, Fields_Fields.NAME, task.Name);
            putInMapIfNotNull(data, Fields_Fields.TASK_DEFINITION_KEY, task.TaskDefinitionKey);
            putInMapIfNotNull(data, Fields_Fields.DESCRIPTION, task.Description);
            putInMapIfNotNull(data, Fields_Fields.ASSIGNEE, task.Assignee);
            putInMapIfNotNull(data, Fields_Fields.OWNER, task.Owner);
            putInMapIfNotNull(data, Fields_Fields.CATEGORY, task.Category);
            putInMapIfNotNull(data, Fields_Fields.CREATE_TIME, task.CreateTime);
            putInMapIfNotNull(data, Fields_Fields.DUE_DATE, task.DueDate);
            putInMapIfNotNull(data, Fields_Fields.FORM_KEY, task.FormKey);
            putInMapIfNotNull(data, Fields_Fields.PRIORITY, task.Priority);
            putInMapIfNotNull(data, Fields_Fields.PROCESS_DEFINITION_ID, task.ProcessDefinitionId);
            putInMapIfNotNull(data, Fields_Fields.PROCESS_INSTANCE_ID, task.ProcessInstanceId);
            putInMapIfNotNull(data, Fields_Fields.EXECUTION_ID, task.ExecutionId);

            if (!string.ReferenceEquals(task.TenantId, null) && !ProcessEngineConfigurationImpl.NO_TENANT_ID.Equals(task.TenantId))
            {
                putInMapIfNotNull(data, Fields_Fields.TENANT_ID, task.TenantId); // Important for standalone tasks
            }
            return data;
        }

    }

}