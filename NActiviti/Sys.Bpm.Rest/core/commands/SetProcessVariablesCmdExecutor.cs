using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class SetProcessVariablesCmdExecutor implements CommandExecutor<org.activiti.cloud.services.api.commands.SetProcessVariablesCmd>
    public class SetProcessVariablesCmdExecutor : CommandExecutor<SetProcessVariablesCmd>
    {

        private ProcessEngineWrapper processEngine;
        private MessageChannel<SetProcessVariablesResults> commandResults;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public SetProcessVariablesCmdExecutor(org.activiti.cloud.services.core.ProcessEngineWrapper processEngine, org.springframework.messaging.MessageChannel commandResults)
        public SetProcessVariablesCmdExecutor(ProcessEngineWrapper processEngine, MessageChannel<SetProcessVariablesResults> commandResults)
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