using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class CompleteTaskCmdExecutor implements CommandExecutor<org.activiti.cloud.services.api.commands.CompleteTaskCmd>
    public class CompleteTaskCmdExecutor : CommandExecutor<CompleteTaskCmd>
    {

        private ProcessEngineWrapper processEngine;
        private MessageChannel<CompleteTaskResults> commandResults;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public CompleteTaskCmdExecutor(org.activiti.cloud.services.core.ProcessEngineWrapper processEngine, org.springframework.messaging.MessageChannel commandResults)
        public CompleteTaskCmdExecutor(ProcessEngineWrapper processEngine, MessageChannel<CompleteTaskResults> commandResults)
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