using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    public class SetProcessVariablesCmdExecutor : CommandExecutor<SetProcessVariablesCmd>
    {

        private ProcessEngineWrapper processEngine;
        private IMessageChannel<SetProcessVariablesResults> commandResults;

        public SetProcessVariablesCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<SetProcessVariablesResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(SetProcessVariablesCmd);
            }
        }

        public virtual void execute(SetProcessVariablesCmd cmd)
        {
            processEngine.ProcessVariables = cmd;
            SetProcessVariablesResults cmdResult = new SetProcessVariablesResults(cmd.Id);
            commandResults.send(MessageBuilder<SetProcessVariablesResults>.withPayload(cmdResult).build());
        }
    }
}