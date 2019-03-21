using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    /// <summary>
    /// 
    /// </summary>
    public class SignalProcessInstancesCmdExecutor : CommandExecutor<SignalCmd>
    {

        private ProcessEngineWrapper processEngine;
        private IMessageChannel<SignalProcessInstancesResults> commandResults;

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
        public virtual void execute(SignalCmd cmd)
        {
            processEngine.signal(cmd);
            SignalProcessInstancesResults cmdResult = new SignalProcessInstancesResults(cmd.Id);
            commandResults.send(MessageBuilder<SignalProcessInstancesResults>.withPayload(cmdResult).build());
        }
    }

}