using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.activiti.cloud.services.api.model;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    public class StartProcessInstanceCmdExecutor : CommandExecutor<StartProcessInstanceCmd>
    {
        private ProcessEngineWrapper processEngine;
        private IMessageChannel<StartProcessInstanceResults> commandResults;

        public StartProcessInstanceCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<StartProcessInstanceResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(StartProcessInstanceCmd);
            }
        }

        public virtual void execute(StartProcessInstanceCmd cmd)
        {
            ProcessInstance processInstance = processEngine.startProcess(cmd);
            if (processInstance != null)
            {
                StartProcessInstanceResults cmdResult = new StartProcessInstanceResults(cmd.Id, processInstance);
                commandResults.send(MessageBuilder<StartProcessInstanceResults>.withPayload(cmdResult).build());
            }
            else
            {
                throw new System.InvalidOperationException("Failed to start processInstance");
            }
        }
    }

}