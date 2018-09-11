using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class RemoveProcessVariablesCmdExecutor implements CommandExecutor<org.activiti.cloud.services.api.commands.RemoveProcessVariablesCmd>
    public class RemoveProcessVariablesCmdExecutor : CommandExecutor<RemoveProcessVariablesCmd>
    {

        private ProcessEngineWrapper processEngine;
        private MessageChannel<RemoveProcessVariablesResults> commandResults;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public RemoveProcessVariablesCmdExecutor(org.activiti.cloud.services.core.ProcessEngineWrapper processEngine, org.springframework.messaging.MessageChannel commandResults)
        public RemoveProcessVariablesCmdExecutor(ProcessEngineWrapper processEngine, MessageChannel<RemoveProcessVariablesResults> commandResults)
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