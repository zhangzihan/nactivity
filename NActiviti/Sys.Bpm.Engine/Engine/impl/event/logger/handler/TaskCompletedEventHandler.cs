using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Events.Logger.Handlers
{

    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    public class TaskCompletedEventHandler : AbstractTaskEventHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        public override IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {

            IActivitiEntityEvent activitiEntityEvent = (IActivitiEntityEvent)@event;

            ITaskEntity task = (ITaskEntity)activitiEntityEvent.Entity;
            IDictionary<string, object> data = handleCommonTaskFields(task);

            long duration = timeStamp.Value.Ticks - task.CreateTime.Value.Ticks;
            PutInMapIfNotNull(data, FieldsFields.DURATION, duration);

            if (@event is IActivitiEntityWithVariablesEvent activitiEntityWithVariablesEvent)
            {
                if (activitiEntityWithVariablesEvent.Variables is object && activitiEntityWithVariablesEvent.Variables.Count > 0)
                {
                    IDictionary<string, object> variableMap = new Dictionary<string, object>();
                    foreach (object variableName in activitiEntityWithVariablesEvent.Variables.Keys)
                    {
                        PutInMapIfNotNull(variableMap, (string)variableName, activitiEntityWithVariablesEvent.Variables[variableName.ToString()]);
                    }
                    if (activitiEntityWithVariablesEvent.LocalScope)
                    {
                        PutInMapIfNotNull(data, FieldsFields.LOCAL_VARIABLES, variableMap);
                    }
                    else
                    {
                        PutInMapIfNotNull(data, FieldsFields.VARIABLES, variableMap);
                    }
                }

            }

            return CreateEventLogEntry(task.ProcessDefinitionId, task.ProcessInstanceId, task.ExecutionId, task.Id, data);
        }

    }

}