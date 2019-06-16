using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace Sys.Workflow.cloud.services.core.commands
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