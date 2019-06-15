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
        private readonly IBinderAwareChannelResolver resolver;

        private readonly IRuntimeService runtimeService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signalCmd"></param>
        public virtual void Receive(SignalCmd signalCmd)
        {
            if ((signalCmd.InputVariables == null) || (signalCmd.InputVariables.Count == 0))
            {
                runtimeService.SignalEventReceived(signalCmd.Name);
            }
            else
            {
                runtimeService.SignalEventReceived(signalCmd.Name, signalCmd.InputVariables);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signalCmd"></param>
        public virtual void BroadcastSignal(SignalCmd signalCmd)
        {
            IMessage<SignalCmd> message = MessageBuilder<SignalCmd>.WithPayload(signalCmd).Build();
            resolver.ResolveDestination("signalEvent").Send<SignalCmd>(message);
        }
    }
}