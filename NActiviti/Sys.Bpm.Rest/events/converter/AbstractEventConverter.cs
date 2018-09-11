using org.activiti.cloud.services.events.configuration;
using org.activiti.engine.@delegate.@event;

namespace org.activiti.cloud.services.events.converter
{
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public abstract class AbstractEventConverter implements EventConverter
    public abstract class AbstractEventConverter : EventConverter
    {
        public abstract string handledType();
        public abstract org.activiti.cloud.services.api.events.ProcessEngineEvent from(IActivitiEvent @event);

        private RuntimeBundleProperties runtimeBundleProperties;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public AbstractEventConverter(org.activiti.cloud.services.events.configuration.RuntimeBundleProperties runtimeBundleProperties)
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