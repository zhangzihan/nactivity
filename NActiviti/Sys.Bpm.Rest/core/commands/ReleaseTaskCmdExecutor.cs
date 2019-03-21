using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    /// <summary>
    /// 
    /// </summary>
    public class ReleaseTaskCmdExecutor : CommandExecutor<ReleaseTaskCmd>
    {

        private ProcessEngineWrapper processEngine;
        private IMessageChannel<ReleaseTaskResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngine"></param>
        /// <param name="commandResults"></param>
        public ReleaseTaskCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<ReleaseTaskResults> commandResults)
        {
            this.processEngine = processEngine;
            this.commandResults = commandResults;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Type HandledType
        {
            get
            {
                return typeof(ReleaseTaskCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void execute(ReleaseTaskCmd cmd)
        {
            processEngine.releaseTask(cmd);
            ReleaseTaskResults cmdResult = new ReleaseTaskResults(cmd.Id);
            commandResults.send(MessageBuilder<ReleaseTaskResults>.withPayload(cmdResult).build());
        }
    }

}