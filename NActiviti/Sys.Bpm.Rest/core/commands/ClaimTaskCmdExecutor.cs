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
    public class ClaimTaskCmdExecutor : CommandExecutor<ClaimTaskCmd>
    {
        private ProcessEngineWrapper processEngine;
        private IMessageChannel<ClaimTaskResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        public ClaimTaskCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<ClaimTaskResults> commandResults)
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
                return typeof(ClaimTaskCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void execute(ClaimTaskCmd cmd)
        {
            processEngine.claimTask(cmd);
            ClaimTaskResults cmdResult = new ClaimTaskResults(cmd.Id);
            commandResults.send(MessageBuilder<ClaimTaskResults>.withPayload(cmdResult).build());
        }
    }

}