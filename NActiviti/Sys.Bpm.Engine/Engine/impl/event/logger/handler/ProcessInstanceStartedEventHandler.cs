using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.@event.logger.handler
{

    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// 
    public class ProcessInstanceStartedEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        private const string TYPE = "PROCESSINSTANCE_START";

        public override IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {

            IActivitiEntityWithVariablesEvent eventWithVariables = (IActivitiEntityWithVariablesEvent)@event;
            IExecutionEntity processInstanceEntity = (IExecutionEntity)eventWithVariables.Entity;

            IDictionary<string, object> data = new Dictionary<string, object>();
            PutInMapIfNotNull(data, FieldsFields.ID, processInstanceEntity.Id);
            PutInMapIfNotNull(data, FieldsFields.BUSINESS_KEY, processInstanceEntity.BusinessKey);
            PutInMapIfNotNull(data, FieldsFields.PROCESS_DEFINITION_ID, processInstanceEntity.ProcessDefinitionId);
            PutInMapIfNotNull(data, FieldsFields.NAME, processInstanceEntity.Name);
            PutInMapIfNotNull(data, FieldsFields.CREATE_TIME, timeStamp);

            if (eventWithVariables.Variables != null && eventWithVariables.Variables.Count > 0)
            {
                IDictionary<string, object> variableMap = new Dictionary<string, object>();
                foreach (object variableName in eventWithVariables.Variables.Keys)
                {
                    PutInMapIfNotNull(variableMap, (string)variableName, eventWithVariables.Variables[variableName.ToString()]);
                }
                PutInMapIfNotNull(data, FieldsFields.VARIABLES, variableMap);
            }

            return CreateEventLogEntry(TYPE, processInstanceEntity.ProcessDefinitionId, processInstanceEntity.Id, null, null, data);
        }

    }

}