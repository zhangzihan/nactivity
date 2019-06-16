using System;
using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.@event.logger
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.@event.logger.handler;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.runtime;
    using Sys.Bpm;
    using Sys.Workflow;

    /// 
    public class EventLogger : IActivitiEventListener
    {
        private const string EVENT_FLUSHER_KEY = "eventFlusher";

        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<EventLogger>();

        protected internal IClock clock;
        protected internal ObjectMapper objectMapper;

        // Mapping of type -> handler
        protected internal IDictionary<ActivitiEventType, Type> eventHandlers = new Dictionary<ActivitiEventType, Type>();

        // Listeners for new events
        protected internal IList<IEventLoggerListener> listeners;

        public EventLogger()
        {
            InitializeDefaultHandlers();
        }

        public EventLogger(IClock clock, ObjectMapper objectMapper) : this()
        {
            this.clock = clock;
            this.objectMapper = objectMapper;
        }

        protected internal virtual void InitializeDefaultHandlers()
        {
            AddEventHandler(ActivitiEventType.TASK_CREATED, typeof(TaskCreatedEventHandler));
            AddEventHandler(ActivitiEventType.TASK_COMPLETED, typeof(TaskCompletedEventHandler));
            AddEventHandler(ActivitiEventType.TASK_ASSIGNED, typeof(TaskAssignedEventHandler));

            AddEventHandler(ActivitiEventType.SEQUENCEFLOW_TAKEN, typeof(SequenceFlowTakenEventHandler));

            AddEventHandler(ActivitiEventType.ACTIVITY_COMPLETED, typeof(ActivityCompletedEventHandler));
            AddEventHandler(ActivitiEventType.ACTIVITY_STARTED, typeof(ActivityStartedEventHandler));
            AddEventHandler(ActivitiEventType.ACTIVITY_SIGNALED, typeof(ActivitySignaledEventHandler));
            AddEventHandler(ActivitiEventType.ACTIVITY_MESSAGE_RECEIVED, typeof(ActivityMessageEventHandler));
            AddEventHandler(ActivitiEventType.ACTIVITY_COMPENSATE, typeof(ActivityCompensatedEventHandler));
            AddEventHandler(ActivitiEventType.ACTIVITY_ERROR_RECEIVED, typeof(ActivityErrorReceivedEventHandler));

            AddEventHandler(ActivitiEventType.VARIABLE_CREATED, typeof(VariableCreatedEventHandler));
            AddEventHandler(ActivitiEventType.VARIABLE_DELETED, typeof(VariableDeletedEventHandler));
            AddEventHandler(ActivitiEventType.VARIABLE_UPDATED, typeof(VariableUpdatedEventHandler));
        }

        public virtual void OnEvent(IActivitiEvent @event)
        {
            IEventLoggerEventHandler eventHandler = GetEventHandler(@event);
            if (eventHandler != null)
            {

                // Events are flushed when command context is closed
                ICommandContext currentCommandContext = Context.CommandContext;
                IEventFlusher eventFlusher = (IEventFlusher)currentCommandContext.GetAttribute(EVENT_FLUSHER_KEY);

                if (eventFlusher == null)
                {

                    eventFlusher = CreateEventFlusher();
                    if (eventFlusher == null)
                    {
                        eventFlusher = new DatabaseEventFlusher(); // Default
                    }
                    currentCommandContext.AddAttribute(EVENT_FLUSHER_KEY, eventFlusher);

                    currentCommandContext.AddCloseListener(eventFlusher);
                    currentCommandContext.AddCloseListener(new CommandContextCloseListenerAnonymousInnerClass(this));
                }

                eventFlusher.AddEventHandler(eventHandler);
            }
        }

        private class CommandContextCloseListenerAnonymousInnerClass : ICommandContextCloseListener
        {
            private readonly EventLogger outerInstance;

            public CommandContextCloseListenerAnonymousInnerClass(EventLogger outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public virtual void Closing(ICommandContext commandContext)
            {
            }

            public virtual void Closed(ICommandContext commandContext)
            {
                // For those who are interested: we can now broadcast the events were added
                if (outerInstance.listeners != null)
                {
                    foreach (IEventLoggerListener listener in outerInstance.listeners)
                    {
                        listener.EventsAdded(outerInstance);
                    }
                }
            }

            public virtual void AfterSessionsFlush(ICommandContext commandContext)
            {
            }

            public virtual void CloseFailure(ICommandContext commandContext)
            {
            }

        }

        // Subclasses can override this if defaults are not ok
        protected internal virtual IEventLoggerEventHandler GetEventHandler(IActivitiEvent @event)
        {

            Type eventHandlerClass = null;
            if (@event.Type.Equals(ActivitiEventType.ENTITY_INITIALIZED))
            {
                object entity = ((IActivitiEntityEvent)@event).Entity;
                if (entity is IExecutionEntity executionEntity)
                {
                    if (executionEntity.ProcessInstanceId.Equals(executionEntity.Id))
                    {
                        eventHandlerClass = typeof(ProcessInstanceStartedEventHandler);
                    }
                }
            }
            else if (@event.Type.Equals(ActivitiEventType.ENTITY_DELETED))
            {
                object entity = ((IActivitiEntityEvent)@event).Entity;
                if (entity is IExecutionEntity executionEntity)
                {
                    if (executionEntity.ProcessInstanceId.Equals(executionEntity.Id))
                    {
                        eventHandlerClass = typeof(ProcessInstanceEndedEventHandler);
                    }
                }
            }
            else
            {
                // Default: dedicated mapper for the type
                eventHandlerClass = eventHandlers[@event.Type];
            }

            if (eventHandlerClass != null)
            {
                return InstantiateEventHandler(@event, eventHandlerClass);
            }

            return null;
        }

        protected internal virtual IEventLoggerEventHandler InstantiateEventHandler(IActivitiEvent @event, Type eventHandlerClass)
        {
            try
            {
                IEventLoggerEventHandler eventHandler = Activator.CreateInstance(eventHandlerClass) as IEventLoggerEventHandler;
                eventHandler.TimeStamp = clock.CurrentTime;
                eventHandler.Event = @event;
                eventHandler.ObjectMapper = objectMapper;
                return eventHandler;
            }
            catch (Exception)
            {
                log.LogWarning("Could not instantiate " + eventHandlerClass + ", this is most likely a programmatic error");
            }
            return null;
        }

        public virtual bool FailOnException
        {
            get
            {
                return false;
            }
        }

        public virtual void AddEventHandler(ActivitiEventType eventType, Type eventHandlerClass)
        {
            eventHandlers[eventType] = eventHandlerClass;
        }

        public virtual void AddEventLoggerListener(IEventLoggerListener listener)
        {
            if (listeners == null)
            {
                listeners = new List<IEventLoggerListener>(1);
            }
            listeners.Add(listener);
        }

        /// <summary>
        /// Subclasses that want something else than the database flusher should override this method
        /// </summary>
        protected internal virtual IEventFlusher CreateEventFlusher()
        {
            return null;
        }

        public virtual IClock Clock
        {
            get
            {
                return clock;
            }
            set
            {
                this.clock = value;
            }
        }


        public virtual ObjectMapper ObjectMapper
        {
            get
            {
                return objectMapper;
            }
            set
            {
                this.objectMapper = value;
            }
        }


        public virtual IList<IEventLoggerListener> Listeners
        {
            get
            {
                return listeners;
            }
            set
            {
                this.listeners = value;
            }
        }
    }
}