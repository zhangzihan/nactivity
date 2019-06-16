using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Events.Logger.Handlers
{

    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    public class ProcessInstanceEndedEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        private const string TYPE = "PROCESSINSTANCE_END";

        public override IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IExecutionEntity processInstanceEntity = GetEntityFromEvent<IExecutionEntity>();

            IDictionary<string, object> data = new Dictionary<string, object>();
            PutInMapIfNotNull(data, FieldsFields.ID, processInstanceEntity.Id);
            PutInMapIfNotNull(data, FieldsFields.BUSINESS_KEY, processInstanceEntity.BusinessKey);
            PutInMapIfNotNull(data, FieldsFields.PROCESS_DEFINITION_ID, processInstanceEntity.ProcessDefinitionId);
            PutInMapIfNotNull(data, FieldsFields.NAME, processInstanceEntity.Name);
            PutInMapIfNotNull(data, FieldsFields.END_TIME, timeStamp);

            return CreateEventLogEntry(TYPE, processInstanceEntity.ProcessDefinitionId, processInstanceEntity.Id, null, null, data);
        }

    }

}