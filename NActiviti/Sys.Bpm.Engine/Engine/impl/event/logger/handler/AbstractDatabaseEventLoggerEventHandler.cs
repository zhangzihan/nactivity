using System;
using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.@event.logger.handler
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.identity;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.impl.util;
    using Sys.Workflow.engine.repository;
    using Sys.Bpm;
    using Sys.Workflow;
    using Sys.Workflow.engine.impl.interceptor;

    /// 
    public abstract class AbstractDatabaseEventLoggerEventHandler : IEventLoggerEventHandler
    {
        private static readonly ILogger<AbstractDatabaseEventLoggerEventHandler> log = ProcessEngineServiceProvider.LoggerService<AbstractDatabaseEventLoggerEventHandler>();
        public abstract IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext);

        protected internal IActivitiEvent @event;
        protected internal DateTime? timeStamp;
        protected internal ObjectMapper objectMapper;

        public AbstractDatabaseEventLoggerEventHandler()
        {
        }

        protected internal virtual IEventLogEntryEntity CreateEventLogEntry(IDictionary<string, object> data)
        {
            return CreateEventLogEntry(null, null, null, null, data);
        }

        protected internal virtual IEventLogEntryEntity CreateEventLogEntry(string processDefinitionId, string processInstanceId, string executionId, string taskId, IDictionary<string, object> data)
        {
            return CreateEventLogEntry(@event.Type.ToString(), processDefinitionId, processInstanceId, executionId, taskId, data);
        }

        protected internal virtual IEventLogEntryEntity CreateEventLogEntry(string type, string processDefinitionId, string processInstanceId, string executionId, string taskId, IDictionary<string, object> data)
        {

            IEventLogEntryEntity eventLogEntry = Context.CommandContext.EventLogEntryEntityManager.Create();
            eventLogEntry.ProcessDefinitionId = processDefinitionId;
            eventLogEntry.ProcessInstanceId = processInstanceId;
            eventLogEntry.ExecutionId = executionId;
            eventLogEntry.TaskId = taskId;
            eventLogEntry.Type = type;
            eventLogEntry.TimeStamp = timeStamp;
            PutInMapIfNotNull(data, FieldsFields.TIMESTAMP, timeStamp);

            // Current user
            string userId = Authentication.AuthenticatedUser.Id;
            if (!(userId is null))
            {
                eventLogEntry.UserId = userId;
                PutInMapIfNotNull(data, "userId", userId);
            }

            // Current tenant
            if (!data.ContainsKey(FieldsFields.TENANT_ID) && !string.IsNullOrWhiteSpace(processDefinitionId))
            {
                IProcessDefinition processDefinition = ProcessDefinitionUtil.GetProcessDefinition(processDefinitionId);
                if (processDefinition != null && !ProcessEngineConfiguration.NO_TENANT_ID.Equals(processDefinition.TenantId))
                {
                    PutInMapIfNotNull(data, FieldsFields.TENANT_ID, processDefinition.TenantId);
                }
            }

            try
            {
                eventLogEntry.Data = objectMapper.WriteValueAsBytes(data);
            }
            catch (Exception e)
            {
                log.LogWarning(e, "Could not serialize event data. Data will not be written to the database");
            }

            return eventLogEntry;

        }

        public virtual IActivitiEvent Event
        {
            set
            {
                this.@event = value;
            }
        }

        public virtual DateTime? TimeStamp
        {
            set
            {
                this.timeStamp = value;
            }
        }

        public virtual ObjectMapper ObjectMapper
        {
            set
            {
                this.objectMapper = value;
            }
        }

        // Helper methods //////////////////////////////////////////////////////
        public virtual T GetEntityFromEvent<T>()
        {
            return (T)((IActivitiEntityEvent)@event).Entity;
        }

        public virtual void PutInMapIfNotNull(IDictionary<string, object> map, string key, object value)
        {
            if (value != null)
            {
                map[key] = value;
            }
        }

    }

}