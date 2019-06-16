using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Commands.Results;
using Sys.Workflow.Messaging;
using Sys.Workflow.Messaging.Support;
using System;

namespace Sys.Workflow.Cloud.Services.Core.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class SignalProcessInstancesCmdExecutor : ICommandExecutor<SignalCmd>
    {

        private readonly ProcessEngineWrapper processEngine;
        private readonly IMessageChannel<SignalProcessInstancesResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngine"></param>
        /// <param name="commandResults"></param>
        public SignalProcessInstancesCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<SignalProcessInstancesResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Type HandledType
        {
            get
            {
                return typeof(SignalCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void Execute(SignalCmd cmd)
        {
            processEngine.Signal(cmd);
            SignalProcessInstancesResults cmdResult = new SignalProcessInstancesResults(cmd.Id);
            commandResults.Send(MessageBuilder<SignalProcessInstancesResults>.WithPayload(cmdResult).Build());
        }
    }

}