using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    public class ReleaseTaskCmdExecutor : CommandExecutor<ReleaseTaskCmd>
    {

        private ProcessEngineWrapper processEngine;
        private IMessageChannel<ReleaseTaskResults> commandResults;

        public ReleaseTaskCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<ReleaseTaskResults> commandResults)
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