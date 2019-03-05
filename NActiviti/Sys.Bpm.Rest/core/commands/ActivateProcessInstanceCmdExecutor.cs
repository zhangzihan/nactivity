using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    public class ActivateProcessInstanceCmdExecutor : CommandExecutor<ActivateProcessInstanceCmd>
    {

        private ProcessEngineWrapper processEngine;
        private IMessageChannel<ActivateProcessInstanceResults> commandResults;

        public ActivateProcessInstanceCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<ActivateProcessInstanceResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(ActivateProcessInstanceCmd);
            }
        }

        public virtual void execute(ActivateProcessInstanceCmd cmd)
        {
            processEngine.activate(cmd);
            ActivateProcessInstanceResults cmdResult = new ActivateProcessInstanceResults(cmd.Id);
            commandResults.send(MessageBuilder<ActivateProcessInstanceResults>.withPayload(cmdResult).build());
        }
    }
}