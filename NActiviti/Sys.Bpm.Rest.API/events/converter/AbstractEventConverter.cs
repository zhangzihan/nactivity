using Sys.Workflow.cloud.services.api.events;
using Sys.Workflow.cloud.services.events.configuration;
using Sys.Workflow.engine.@delegate.@event;

namespace Sys.Workflow.cloud.services.events.converter
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractEventConverter : IEventConverter
    {

        /// <summary>
        /// 
        /// </summary>
        public abstract string HandledType();


        /// <summary>
        /// 
        /// </summary>
        public abstract IProcessEngineEvent From(IActivitiEvent @event);


        /// <summary>
        /// 
        /// </summary>
        private readonly RuntimeBundleProperties runtimeBundleProperties;


        /// <summary>
        /// 
        /// </summary>
        public AbstractEventConverter(RuntimeBundleProperties runtimeBundleProperties)
        {
            this.runtimeBundleProperties = runtimeBundleProperties;
        }

        /// <summary>
        /// 
        /// </summary>

        protected internal virtual RuntimeBundleProperties RuntimeBundleProperties
        {
            get
            {
                return runtimeBundleProperties;
            }
        }
    }

}