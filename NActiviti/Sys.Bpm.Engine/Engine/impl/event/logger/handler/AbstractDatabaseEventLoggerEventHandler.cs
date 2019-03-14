using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.identity;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.repository;
    using Sys;
    using Sys.Bpm;

    /// 
    public abstract class AbstractDatabaseEventLoggerEventHandler : IEventLoggerEventHandler
    {
        private static readonly ILogger<AbstractDatabaseEventLoggerEventHandler> log = ProcessEngineServiceProvider.LoggerService<AbstractDatabaseEventLoggerEventHandler>();
        public abstract IEventLogEntryEntity generateEventLogEntry(org.activiti.engine.impl.interceptor.CommandContext<IEventLogEntryEntity> commandContext);

        protected internal IActivitiEvent @event;
        protected internal DateTime? timeStamp;
        protected internal ObjectMapper objectMapper;

        public AbstractDatabaseEventLoggerEventHandler()
        {
        }

        protected internal virtual IEventLogEntryEntity createEventLogEntry(IDictionary<string, object> data)
        {
            return createEventLogEntry(null, null, null, null, data);
        }

        protected internal virtual IEventLogEntryEntity createEventLogEntry(string processDefinitionId, string processInstanceId, string executionId, string taskId, IDictionary<string, object> data)
        {
            return createEventLogEntry(@event.Type.ToString(), processDefinitionId, processInstanceId, executionId, taskId, data);
        }

        protected internal virtual IEventLogEntryEntity createEventLogEntry(string type, string processDefinitionId, string processInstanceId, string executionId, string taskId, IDictionary<string, object> data)
        {

            IEventLogEntryEntity eventLogEntry = Context.CommandContext.EventLogEntryEntityManager.create();
            eventLogEntry.ProcessDefinitionId = processDefinitionId;
            eventLogEntry.ProcessInstanceId = processInstanceId;
            eventLogEntry.ExecutionId = executionId;
            eventLogEntry.TaskId = taskId;
            eventLogEntry.Type = type;
            eventLogEntry.TimeStamp = timeStamp;
            putInMapIfNotNull(data, Fields_Fields.TIMESTAMP, timeStamp);

            // Current user
            string userId = Authentication.AuthenticatedUser.Id;
            if (!ReferenceEquals(userId, null))
            {
                eventLogEntry.UserId = userId;
                putInMapIfNotNull(data, "userId", userId);
            }

            // Current tenant
            if (!data.ContainsKey(Fields_Fields.TENANT_ID) && !string.IsNullOrWhiteSpace(processDefinitionId))
            {
                IProcessDefinition processDefinition = ProcessDefinitionUtil.getProcessDefinition(processDefinitionId);
                if (processDefinition != null && !ProcessEngineConfiguration.NO_TENANT_ID.Equals(processDefinition.TenantId))
                {
                    putInMapIfNotNull(data, Fields_Fields.TENANT_ID, processDefinition.TenantId);
                }
            }

            try
            {
                eventLogEntry.Data = objectMapper.writeValueAsBytes(data);
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
        public virtual T getEntityFromEvent<T>()
        {
            return (T)((IActivitiEntityEvent)@event).Entity;
        }

        public virtual void putInMapIfNotNull(IDictionary<string, object> map, string key, object value)
        {
            if (value != null)
            {
                map[key] = value;
            }
        }

    }

}