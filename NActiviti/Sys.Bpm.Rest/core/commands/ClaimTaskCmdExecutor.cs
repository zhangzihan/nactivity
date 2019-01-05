using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    public class ClaimTaskCmdExecutor : CommandExecutor<ClaimTaskCmd>
    {
        private ProcessEngineWrapper processEngine;
        private MessageChannel<ClaimTaskResults> commandResults;

        public ClaimTaskCmdExecutor(ProcessEngineWrapper processEngine, MessageChannel<ClaimTaskResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(ClaimTaskCmd);
            }
        }

        public virtual void execute(ClaimTaskCmd cmd)
        {
            processEngine.claimTask(cmd);
            ClaimTaskResults cmdResult = new ClaimTaskResults(cmd.Id);
            commandResults.send(MessageBuilder<ClaimTaskResults>.withPayload(cmdResult).build());
        }
    }

}