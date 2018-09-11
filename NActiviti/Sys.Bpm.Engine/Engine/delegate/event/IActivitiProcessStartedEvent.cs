namespace org.activiti.engine.@delegate.@event
{
    /// <summary>
    /// An <seealso cref="org.activiti.engine.delegate.event.ActivitiEvent"/> related to start event being sent when activiti process
    /// instance is started.
    /// 
    /// 
    /// </summary>
    public interface IActivitiProcessStartedEvent : IActivitiEntityWithVariablesEvent
    {

        /// <returns> the id of the process instance of the nested process that starts the current process instance, or null if
        ///         the current process instance is not started into a nested process. </returns>
        string NestedProcessInstanceId { get; }

        /// <returns> the id of the process definition of the nested process that starts the current process instance, or null
        ///         if the current process instance is not started into a nested process. </returns>
        string NestedProcessDefinitionId { get; }
    }

}