using Sys.Workflow.engine.@delegate.@event;

namespace Sys.Workflow.engine.impl.@event.logger
{
    /// 
    public interface IEventLoggerListener
    {

        void EventsAdded(IActivitiEventListener databaseEventLogger);
    }
}