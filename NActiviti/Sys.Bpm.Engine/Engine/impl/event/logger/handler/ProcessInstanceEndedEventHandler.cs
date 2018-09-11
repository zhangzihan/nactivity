using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class ProcessInstanceEndedEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        private const string TYPE = "PROCESSINSTANCE_END";

        public override IEventLogEntryEntity generateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IExecutionEntity processInstanceEntity = getEntityFromEvent<IExecutionEntity>();

            IDictionary<string, object> data = new Dictionary<string, object>();
            putInMapIfNotNull(data, Fields_Fields.ID, processInstanceEntity.Id);
            putInMapIfNotNull(data, Fields_Fields.BUSINESS_KEY, processInstanceEntity.BusinessKey);
            putInMapIfNotNull(data, Fields_Fields.PROCESS_DEFINITION_ID, processInstanceEntity.ProcessDefinitionId);
            putInMapIfNotNull(data, Fields_Fields.NAME, processInstanceEntity.Name);
            putInMapIfNotNull(data, Fields_Fields.END_TIME, timeStamp);

            return createEventLogEntry(TYPE, processInstanceEntity.ProcessDefinitionId, processInstanceEntity.Id, null, null, data);
        }

    }

}