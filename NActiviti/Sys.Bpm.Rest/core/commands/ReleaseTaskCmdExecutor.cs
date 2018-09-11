using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{

    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class ReleaseTaskCmdExecutor implements CommandExecutor<org.activiti.cloud.services.api.commands.ReleaseTaskCmd>
    public class ReleaseTaskCmdExecutor : CommandExecutor<ReleaseTaskCmd>
    {

        private ProcessEngineWrapper processEngine;
        private MessageChannel<ReleaseTaskResults> commandResults;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public ReleaseTaskCmdExecutor(org.activiti.cloud.services.core.ProcessEngineWrapper processEngine, org.springframework.messaging.MessageChannel commandResults)
        public ReleaseTaskCmdExecutor(ProcessEngineWrapper processEngine, MessageChannel<ReleaseTaskResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        public virtual Type HandledType
        {
            get
            {
                return typeof(ReleaseTaskCmd);
            }
        }

        public virtual void execute(ReleaseTaskCmd cmd)
        {
            processEngine.releaseTask(cmd);
            ReleaseTaskResults cmdResult = new ReleaseTaskResults(cmd.Id);
            commandResults.send(MessageBuilder<ReleaseTaskResults>.withPayload(cmdResult).build());
        }
    }

}