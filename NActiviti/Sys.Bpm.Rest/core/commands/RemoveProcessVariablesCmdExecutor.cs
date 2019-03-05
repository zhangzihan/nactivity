using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    public class RemoveProcessVariablesCmdExecutor : CommandExecutor<RemoveProcessVariablesCmd>
    {

        private ProcessEngineWrapper processEngine;
        private IMessageChannel<RemoveProcessVariablesResults> commandResults;

        public RemoveProcessVariablesCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<RemoveProcessVariablesResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(RemoveProcessVariablesCmd);
            }
        }

        public virtual void execute(RemoveProcessVariablesCmd cmd)
        {
            processEngine.removeProcessVariables(cmd);
            RemoveProcessVariablesResults cmdResult = new RemoveProcessVariablesResults(cmd.Id);
            commandResults.send(MessageBuilder<RemoveProcessVariablesResults>.withPayload(cmdResult).build());
        }
    }
}