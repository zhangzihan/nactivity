using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class ProcessInstanceStartedEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        private const string TYPE = "PROCESSINSTANCE_START";

        public override IEventLogEntryEntity generateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {

            IActivitiEntityWithVariablesEvent eventWithVariables = (IActivitiEntityWithVariablesEvent)@event;
            IExecutionEntity processInstanceEntity = (IExecutionEntity)eventWithVariables.Entity;

            IDictionary<string, object> data = new Dictionary<string, object>();
            putInMapIfNotNull(data, Fields_Fields.ID, processInstanceEntity.Id);
            putInMapIfNotNull(data, Fields_Fields.BUSINESS_KEY, processInstanceEntity.BusinessKey);
            putInMapIfNotNull(data, Fields_Fields.PROCESS_DEFINITION_ID, processInstanceEntity.ProcessDefinitionId);
            putInMapIfNotNull(data, Fields_Fields.NAME, processInstanceEntity.Name);
            putInMapIfNotNull(data, Fields_Fields.CREATE_TIME, timeStamp);

            if (eventWithVariables.Variables != null && eventWithVariables.Variables.Count > 0)
            {
                IDictionary<string, object> variableMap = new Dictionary<string, object>();
                foreach (object variableName in eventWithVariables.Variables.Keys)
                {
                    putInMapIfNotNull(variableMap, (string)variableName, eventWithVariables.Variables[variableName.ToString()]);
                }
                putInMapIfNotNull(data, Fields_Fields.VARIABLES, variableMap);
            }

            return createEventLogEntry(TYPE, processInstanceEntity.ProcessDefinitionId, processInstanceEntity.Id, null, null, data);
        }

    }

}