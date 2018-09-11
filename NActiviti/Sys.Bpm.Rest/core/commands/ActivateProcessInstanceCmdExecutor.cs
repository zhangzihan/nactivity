using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class ActivateProcessInstanceCmdExecutor implements CommandExecutor<org.activiti.cloud.services.api.commands.ActivateProcessInstanceCmd>
    public class ActivateProcessInstanceCmdExecutor : CommandExecutor<ActivateProcessInstanceCmd>
    {

        private ProcessEngineWrapper processEngine;
        private MessageChannel<ActivateProcessInstanceResults> commandResults;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public ActivateProcessInstanceCmdExecutor(org.activiti.cloud.services.core.ProcessEngineWrapper processEngine, org.springframework.messaging.MessageChannel commandResults)
        public ActivateProcessInstanceCmdExecutor(ProcessEngineWrapper processEngine, MessageChannel<ActivateProcessInstanceResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(ActivateProcessInstanceCmd);
            }
        }

        public virtual void execute(ActivateProcessInstanceCmd cmd)
        {
            processEngine.activate(cmd);
            ActivateProcessInstanceResults cmdResult = new ActivateProcessInstanceResults(cmd.Id);
            commandResults.send(MessageBuilder<ActivateProcessInstanceResults>.withPayload(cmdResult).build());
        }
    }
}