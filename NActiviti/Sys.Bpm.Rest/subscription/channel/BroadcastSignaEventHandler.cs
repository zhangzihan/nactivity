using org.activiti.cloud.services.api.commands;
using org.activiti.engine;
using org.springframework.cloud.stream.binding;
using org.springframework.messaging;
using org.springframework.messaging.support;

namespace org.activiti.services.subscription.channel
{
    /// <summary>
    /// 
    /// </summary>
    public class BroadcastSignaEventHandler
    {
        private IBinderAwareChannelResolver resolver;

        private IRuntimeService runtimeService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signalCmd"></param>
        public virtual void receive(SignalCmd signalCmd)
        {
            if ((signalCmd.InputVariables == null) || (signalCmd.InputVariables.Count == 0))
            {
                runtimeService.signalEventReceived(signalCmd.Name);
            }
            else
            {
                runtimeService.signalEventReceived(signalCmd.Name, signalCmd.InputVariables);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signalCmd"></param>
        public virtual void broadcastSignal(SignalCmd signalCmd)
        {
            IMessage<SignalCmd> message = MessageBuilder<SignalCmd>.withPayload(signalCmd).build();
            resolver.resolveDestination("signalEvent").send<SignalCmd>(message);
        }
    }
}