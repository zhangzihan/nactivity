using org.activiti.engine.@delegate.@event;

namespace org.activiti.engine.impl.@event.logger
{
    /// 
    public interface IEventLoggerListener
    {

        void EventsAdded(IActivitiEventListener databaseEventLogger);
    }
}