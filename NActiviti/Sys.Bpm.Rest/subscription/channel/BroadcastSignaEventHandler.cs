using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Engine;
using Sys.Workflow.Cloud.Streams.Bindings;
using Sys.Workflow.Messaging;
using Sys.Workflow.Messaging.Support;

namespace Sys.Workflow.Services.Subscription.Channels
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