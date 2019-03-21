using org.activiti.cloud.services.api.commands;
using org.activiti.services.subscription.channel;

namespace org.activiti.services.subscription
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
        public virtual void sendSignal(SignalCmd signalCmd)
        {
            eventHandler.broadcastSignal(signalCmd);
        }
    }
}