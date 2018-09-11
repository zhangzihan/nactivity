using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class SuspendProcessInstanceCmdExecutor implements CommandExecutor<org.activiti.cloud.services.api.commands.SuspendProcessInstanceCmd>
    public class SuspendProcessInstanceCmdExecutor : CommandExecutor<SuspendProcessInstanceCmd>
    {

        private ProcessEngineWrapper processEngine;
        private MessageChannel<SuspendProcessInstanceResults> commandResults;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public SuspendProcessInstanceCmdExecutor(org.activiti.cloud.services.core.ProcessEngineWrapper processEngine, org.springframework.messaging.MessageChannel commandResults)
        public SuspendProcessInstanceCmdExecutor(ProcessEngineWrapper processEngine, MessageChannel<SuspendProcessInstanceResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(SuspendProcessInstanceCmd);
            }
        }

        public virtual void execute(SuspendProcessInstanceCmd cmd)
        {
            processEngine.suspend(cmd);
            SuspendProcessInstanceResults cmdResult = new SuspendProcessInstanceResults(cmd.Id);
            commandResults.send(MessageBuilder<SuspendProcessInstanceResults>.withPayload(cmdResult).build());
        }
    }

}