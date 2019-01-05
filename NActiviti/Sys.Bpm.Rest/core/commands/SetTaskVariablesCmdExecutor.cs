using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    public class SetTaskVariablesCmdExecutor : CommandExecutor<SetTaskVariablesCmd>
    {

        private ProcessEngineWrapper processEngine;
        private MessageChannel<SetTaskVariablesResults> commandResults;

        public SetTaskVariablesCmdExecutor(ProcessEngineWrapper processEngine, MessageChannel<SetTaskVariablesResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(SetTaskVariablesCmd);
            }
        }

        public virtual void execute(SetTaskVariablesCmd cmd)
        {
            processEngine.TaskVariables = cmd;
            SetTaskVariablesResults cmdResult = new SetTaskVariablesResults(cmd.Id);
            commandResults.send(MessageBuilder<SetTaskVariablesResults>.withPayload(cmdResult).build());
        }
    }

}