using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class ClaimTaskCmdExecutor implements CommandExecutor<org.activiti.cloud.services.api.commands.ClaimTaskCmd>
    public class ClaimTaskCmdExecutor : CommandExecutor<ClaimTaskCmd>
    {
        private ProcessEngineWrapper processEngine;
        private MessageChannel<ClaimTaskResults> commandResults;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public ClaimTaskCmdExecutor(org.activiti.cloud.services.core.ProcessEngineWrapper processEngine, org.springframework.messaging.MessageChannel commandResults)
        public ClaimTaskCmdExecutor(ProcessEngineWrapper processEngine, MessageChannel<ClaimTaskResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(ClaimTaskCmd);
            }
        }

        public virtual void execute(ClaimTaskCmd cmd)
        {
            processEngine.claimTask(cmd);
            ClaimTaskResults cmdResult = new ClaimTaskResults(cmd.Id);
            commandResults.send(MessageBuilder<ClaimTaskResults>.withPayload(cmdResult).build());
        }
    }

}