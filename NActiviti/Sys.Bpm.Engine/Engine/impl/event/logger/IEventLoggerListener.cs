using Sys.Workflow.Engine.Delegate.Events;

namespace Sys.Workflow.Engine.Impl.Events.Logger
{
    /// 
    public interface IEventLoggerListener
    {

        void EventsAdded(IActivitiEventListener databaseEventLogger);
    }
}