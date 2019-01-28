using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger
{
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.@event.logger.handler;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;
    using Sys.Bpm;

    /// 
    public class EventLogger : IActivitiEventListener
    {
        private const string EVENT_FLUSHER_KEY = "eventFlusher";

        protected internal IClock clock;
        protected internal ObjectMapper objectMapper;

        // Mapping of type -> handler
        protected internal IDictionary<ActivitiEventType, Type> eventHandlers = new Dictionary<ActivitiEventType, Type>();

        // Listeners for new events
        protected internal IList<IEventLoggerListener> listeners;

        public EventLogger()
        {
            initializeDefaultHandlers();
        }

        public EventLogger(IClock clock, ObjectMapper objectMapper) : this()
        {
            this.clock = clock;
            this.objectMapper = objectMapper;
        }

        protected internal virtual void initializeDefaultHandlers()
        {
            addEventHandler(ActivitiEventType.TASK_CREATED, typeof(TaskCreatedEventHandler));
            addEventHandler(ActivitiEventType.TASK_COMPLETED, typeof(TaskCompletedEventHandler));
            addEventHandler(ActivitiEventType.TASK_ASSIGNED, typeof(TaskAssignedEventHandler));

            addEventHandler(ActivitiEventType.SEQUENCEFLOW_TAKEN, typeof(SequenceFlowTakenEventHandler));

            addEventHandler(ActivitiEventType.ACTIVITY_COMPLETED, typeof(ActivityCompletedEventHandler));
            addEventHandler(ActivitiEventType.ACTIVITY_STARTED, typeof(ActivityStartedEventHandler));
            addEventHandler(ActivitiEventType.ACTIVITY_SIGNALED, typeof(ActivitySignaledEventHandler));
            addEventHandler(ActivitiEventType.ACTIVITY_MESSAGE_RECEIVED, typeof(ActivityMessageEventHandler));
            addEventHandler(ActivitiEventType.ACTIVITY_COMPENSATE, typeof(ActivityCompensatedEventHandler));
            addEventHandler(ActivitiEventType.ACTIVITY_ERROR_RECEIVED, typeof(ActivityErrorReceivedEventHandler));

            addEventHandler(ActivitiEventType.VARIABLE_CREATED, typeof(VariableCreatedEventHandler));
            addEventHandler(ActivitiEventType.VARIABLE_DELETED, typeof(VariableDeletedEventHandler));
            addEventHandler(ActivitiEventType.VARIABLE_UPDATED, typeof(VariableUpdatedEventHandler));
        }

        public virtual void onEvent(IActivitiEvent @event)
        {
            IEventLoggerEventHandler eventHandler = getEventHandler(@event);
            if (eventHandler != null)
            {

                // Events are flushed when command context is closed
                ICommandContext currentCommandContext = Context.CommandContext;
                IEventFlusher eventFlusher = (IEventFlusher)currentCommandContext.getAttribute(EVENT_FLUSHER_KEY);

                if (eventFlusher == null)
                {

                    eventFlusher = createEventFlusher();
                    if (eventFlusher == null)
                    {
                        eventFlusher = new DatabaseEventFlusher(); // Default
                    }
                    currentCommandContext.addAttribute(EVENT_FLUSHER_KEY, eventFlusher);

                    currentCommandContext.addCloseListener(eventFlusher);
                    currentCommandContext.addCloseListener(new CommandContextCloseListenerAnonymousInnerClass(this));
                }

                eventFlusher.addEventHandler(eventHandler);
            }
        }

        private class CommandContextCloseListenerAnonymousInnerClass : ICommandContextCloseListener
        {
            private readonly EventLogger outerInstance;

            public CommandContextCloseListenerAnonymousInnerClass(EventLogger outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public virtual void closing(ICommandContext commandContext)
            {
            }

            public virtual void closed(ICommandContext commandContext)
            {
                // For those who are interested: we can now broadcast the events were added
                if (outerInstance.listeners != null)
                {
                    foreach (IEventLoggerListener listener in outerInstance.listeners)
                    {
                        listener.eventsAdded(outerInstance);
                    }
                }
            }

            public virtual void afterSessionsFlush(ICommandContext commandContext)
            {
            }

            public virtual void closeFailure(ICommandContext commandContext)
            {
            }

        }

        // Subclasses can override this if defaults are not ok
        protected internal virtual IEventLoggerEventHandler getEventHandler(IActivitiEvent @event)
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
                return instantiateEventHandler(@event, eventHandlerClass);
            }

            return null;
        }

        protected internal virtual IEventLoggerEventHandler instantiateEventHandler(IActivitiEvent @event, Type eventHandlerClass)
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
                //logger.warn("Could not instantiate " + eventHandlerClass + ", this is most likely a programmatic error");
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

        public virtual void addEventHandler(ActivitiEventType eventType, Type eventHandlerClass)
        {
            eventHandlers[eventType] = eventHandlerClass;
        }

        public virtual void addEventLoggerListener(IEventLoggerListener listener)
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
        protected internal virtual IEventFlusher createEventFlusher()
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