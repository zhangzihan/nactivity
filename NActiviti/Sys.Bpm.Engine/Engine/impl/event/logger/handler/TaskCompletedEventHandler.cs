using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class TaskCompletedEventHandler : AbstractTaskEventHandler
    {

        public override IEventLogEntryEntity generateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {

            IActivitiEntityEvent activitiEntityEvent = (IActivitiEntityEvent)@event;

            ITaskEntity task = (ITaskEntity)activitiEntityEvent.Entity;
            IDictionary<string, object> data = handleCommonTaskFields(task);

            long duration = timeStamp.Value.Ticks - task.CreateTime.Value.Ticks;
            putInMapIfNotNull(data, Fields_Fields.DURATION, duration);

            if (@event is IActivitiEntityWithVariablesEvent)
            {
                IActivitiEntityWithVariablesEvent activitiEntityWithVariablesEvent = (IActivitiEntityWithVariablesEvent)@event;
                if (activitiEntityWithVariablesEvent.Variables != null && activitiEntityWithVariablesEvent.Variables.Count > 0)
                {
                    IDictionary<string, object> variableMap = new Dictionary<string, object>();
                    foreach (object variableName in activitiEntityWithVariablesEvent.Variables.Keys)
                    {
                        putInMapIfNotNull(variableMap, (string)variableName, activitiEntityWithVariablesEvent.Variables[variableName.ToString()]);
                    }
                    if (activitiEntityWithVariablesEvent.LocalScope)
                    {
                        putInMapIfNotNull(data, Fields_Fields.LOCAL_VARIABLES, variableMap);
                    }
                    else
                    {
                        putInMapIfNotNull(data, Fields_Fields.VARIABLES, variableMap);
                    }
                }

            }

            return createEventLogEntry(task.ProcessDefinitionId, task.ProcessInstanceId, task.ExecutionId, task.Id, data);
        }

    }

}