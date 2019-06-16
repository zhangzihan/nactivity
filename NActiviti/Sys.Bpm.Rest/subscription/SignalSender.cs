using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.services.subscription.channel;

namespace Sys.Workflow.services.subscription
{
    /// <summary>
    /// 
    /// </summary>
    public class SignalSender
    {
        private readonly BroadcastSignaEventHandler eventHandler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventHandler"></param>
        public SignalSender(BroadcastSignaEventHandler eventHandler)
        {
            this.eventHandler = eventHandler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signalCmd"></param>
        public virtual void SendSignal(SignalCmd signalCmd)
        {
            eventHandler.BroadcastSignal(signalCmd);
        }
    }
}