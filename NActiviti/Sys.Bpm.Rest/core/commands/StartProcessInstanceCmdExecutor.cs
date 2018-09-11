using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.activiti.cloud.services.api.model;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class StartProcessInstanceCmdExecutor implements CommandExecutor<org.activiti.cloud.services.api.commands.StartProcessInstanceCmd>
    public class StartProcessInstanceCmdExecutor : CommandExecutor<StartProcessInstanceCmd>
    {
        private ProcessEngineWrapper processEngine;
        private MessageChannel<StartProcessInstanceResults> commandResults;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public StartProcessInstanceCmdExecutor(org.activiti.cloud.services.core.ProcessEngineWrapper processEngine, org.springframework.messaging.MessageChannel commandResults)
        public StartProcessInstanceCmdExecutor(ProcessEngineWrapper processEngine, MessageChannel<StartProcessInstanceResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(StartProcessInstanceCmd);
            }
        }

        public virtual void execute(StartProcessInstanceCmd cmd)
        {
            ProcessInstance processInstance = processEngine.startProcess(cmd);
            if (processInstance != null)
            {
                StartProcessInstanceResults cmdResult = new StartProcessInstanceResults(cmd.Id, processInstance);
                commandResults.send(MessageBuilder<StartProcessInstanceResults>.withPayload(cmdResult).build());
            }
            else
            {
                throw new System.InvalidOperationException("Failed to start processInstance");
            }
        }
    }

}