using Sys.Workflow.Cloud.Services.Api.Events;
using Sys.Workflow.Cloud.Services.Events.Configurations;
using Sys.Workflow.Engine.Delegate.Events;

namespace Sys.Workflow.Cloud.Services.Events.Converters
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