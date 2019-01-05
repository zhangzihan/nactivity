using org.activiti.cloud.services.events.configuration;
using org.activiti.engine.@delegate.@event;

namespace org.activiti.cloud.services.events.converter
{
    public abstract class AbstractEventConverter : EventConverter
    {
        public abstract string handledType();
        public abstract org.activiti.cloud.services.api.events.ProcessEngineEvent from(IActivitiEvent @event);

        private RuntimeBundleProperties runtimeBundleProperties;

        public AbstractEventConverter(RuntimeBundleProperties runtimeBundleProperties)
        {
            this.runtimeBundleProperties = runtimeBundleProperties;
        }

        protected internal virtual RuntimeBundleProperties RuntimeBundleProperties
        {
            get
            {
                return runtimeBundleProperties;
            }
        }
    }

}