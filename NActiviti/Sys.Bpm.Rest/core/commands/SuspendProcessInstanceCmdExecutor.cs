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
    public class SuspendProcessInstanceCmdExecutor : CommandExecutor<SuspendProcessInstanceCmd>
    {

        private ProcessEngineWrapper processEngine;
        private IMessageChannel<SuspendProcessInstanceResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngine"></param>
        /// <param name="commandResults"></param>
        public SuspendProcessInstanceCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<SuspendProcessInstanceResults> commandResults)
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
                return typeof(SuspendProcessInstanceCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void execute(SuspendProcessInstanceCmd cmd)
        {
            processEngine.suspend(cmd);
            SuspendProcessInstanceResults cmdResult = new SuspendProcessInstanceResults(cmd.Id);
            commandResults.send(MessageBuilder<SuspendProcessInstanceResults>.withPayload(cmdResult).build());
        }
    }
}