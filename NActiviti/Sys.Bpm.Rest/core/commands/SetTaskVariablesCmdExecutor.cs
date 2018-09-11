using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class SetTaskVariablesCmdExecutor implements CommandExecutor<org.activiti.cloud.services.api.commands.SetTaskVariablesCmd>
    public class SetTaskVariablesCmdExecutor : CommandExecutor<SetTaskVariablesCmd>
    {

        private ProcessEngineWrapper processEngine;
        private MessageChannel<SetTaskVariablesResults> commandResults;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public SetTaskVariablesCmdExecutor(org.activiti.cloud.services.core.ProcessEngineWrapper processEngine, org.springframework.messaging.MessageChannel commandResults)
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