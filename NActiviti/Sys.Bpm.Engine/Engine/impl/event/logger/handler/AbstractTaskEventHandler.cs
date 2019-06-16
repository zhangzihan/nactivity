using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Events.Logger.Handlers
{

    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    public abstract class AbstractTaskEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        protected internal virtual IDictionary<string, object> handleCommonTaskFields(ITaskEntity task)
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
            PutInMapIfNotNull(data, FieldsFields.ID, task.Id);
            PutInMapIfNotNull(data, FieldsFields.NAME, task.Name);
            PutInMapIfNotNull(data, FieldsFields.TASK_DEFINITION_KEY, task.TaskDefinitionKey);
            PutInMapIfNotNull(data, FieldsFields.DESCRIPTION, task.Description);
            PutInMapIfNotNull(data, FieldsFields.ASSIGNEE, task.Assignee);
            PutInMapIfNotNull(data, FieldsFields.OWNER, task.Owner);
            PutInMapIfNotNull(data, FieldsFields.CATEGORY, task.Category);
            PutInMapIfNotNull(data, FieldsFields.CREATE_TIME, task.CreateTime);
            PutInMapIfNotNull(data, FieldsFields.DUE_DATE, task.DueDate);
            PutInMapIfNotNull(data, FieldsFields.FORM_KEY, task.FormKey);
            PutInMapIfNotNull(data, FieldsFields.PRIORITY, task.Priority);
            PutInMapIfNotNull(data, FieldsFields.PROCESS_DEFINITION_ID, task.ProcessDefinitionId);
            PutInMapIfNotNull(data, FieldsFields.PROCESS_INSTANCE_ID, task.ProcessInstanceId);
            PutInMapIfNotNull(data, FieldsFields.EXECUTION_ID, task.ExecutionId);

            if (!(task.TenantId is null) && !ProcessEngineConfiguration.NO_TENANT_ID.Equals(task.TenantId))
            {
                PutInMapIfNotNull(data, FieldsFields.TENANT_ID, task.TenantId); // Important for standalone tasks
            }
            return data;
        }

    }

}