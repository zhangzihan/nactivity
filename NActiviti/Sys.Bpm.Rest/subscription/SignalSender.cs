using org.activiti.cloud.services.api.commands;
using org.activiti.services.subscription.channel;

namespace org.activiti.services.subscription
{
    public class SignalSender
    {
        private readonly BroadcastSignaEventHandler eventHandler;

        public SignalSender(BroadcastSignaEventHandler eventHandler)
        {
            this.eventHandler = eventHandler;
        }

        public virtual void sendSignal(SignalCmd signalCmd)
        {
            eventHandler.broadcastSignal(signalCmd);
        }
    }
}