using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Services.Subscription.Channels;

namespace Sys.Workflow.Services.Subscription
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