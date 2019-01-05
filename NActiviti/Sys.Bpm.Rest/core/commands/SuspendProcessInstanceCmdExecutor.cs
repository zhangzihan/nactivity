using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    public class SuspendProcessInstanceCmdExecutor : CommandExecutor<SuspendProcessInstanceCmd>
    {

        private ProcessEngineWrapper processEngine;
        private MessageChannel<SuspendProcessInstanceResults> commandResults;

        public SuspendProcessInstanceCmdExecutor(ProcessEngineWrapper processEngine, MessageChannel<SuspendProcessInstanceResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(SuspendProcessInstanceCmd);
            }
        }

        public virtual void execute(SuspendProcessInstanceCmd cmd)
        {
            processEngine.suspend(cmd);
            SuspendProcessInstanceResults cmdResult = new SuspendProcessInstanceResults(cmd.Id);
            commandResults.send(MessageBuilder<SuspendProcessInstanceResults>.withPayload(cmdResult).build());
        }
    }

}