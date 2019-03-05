using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    public class CompleteTaskCmdExecutor : CommandExecutor<CompleteTaskCmd>
    {

        private ProcessEngineWrapper processEngine;
        private IMessageChannel<CompleteTaskResults> commandResults;

        public CompleteTaskCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<CompleteTaskResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(CompleteTaskCmd);
            }
        }

        public virtual void execute(CompleteTaskCmd cmd)
        {
            processEngine.completeTask(cmd);
            CompleteTaskResults cmdResult = new CompleteTaskResults(cmd.Id);
            commandResults.send(MessageBuilder<CompleteTaskResults>.withPayload(cmdResult).build());
        }
    }

}