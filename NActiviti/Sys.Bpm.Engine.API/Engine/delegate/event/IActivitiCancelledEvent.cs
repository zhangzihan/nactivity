namespace Sys.Workflow.engine.@delegate.@event
{
    /// <summary>
    /// An <seealso cref="Sys.Workflow.engine.delegate.event.ActivitiEvent"/> related to cancel event being sent when activiti object is cancelled.
    /// 
    /// 
    /// </summary>
    public interface IActivitiCancelledEvent : IActivitiEvent
    {
        /// <returns> the cause of the cancel event. Returns null, if no specific cause has been specified. </returns>
        object Cause { get; }
    }

}