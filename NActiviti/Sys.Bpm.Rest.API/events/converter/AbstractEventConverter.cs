using org.activiti.cloud.services.events.configuration;
using org.activiti.engine.@delegate.@event;

namespace org.activiti.cloud.services.events.converter
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractEventConverter : IEventConverter
    {

        /// <summary>
        /// 
        /// </summary>
        public abstract string handledType();


        /// <summary>
        /// 
        /// </summary>
        public abstract org.activiti.cloud.services.api.events.IProcessEngineEvent from(IActivitiEvent @event);


        /// <summary>
        /// 
        /// </summary>
        private RuntimeBundleProperties runtimeBundleProperties;


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