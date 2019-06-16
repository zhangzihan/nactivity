using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace Sys.Workflow.cloud.services.core.commands
{
    /// <summary>
    /// 
    /// </summary>
    public class ActivateProcessInstanceCmdExecutor : ICommandExecutor<ActivateProcessInstanceCmd>
    {

        private readonly ProcessEngineWrapper processEngine;
        private readonly IMessageChannel<ActivateProcessInstanceResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        public ActivateProcessInstanceCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<ActivateProcessInstanceResults> commandResults)
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
                return typeof(ActivateProcessInstanceCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Execute(ActivateProcessInstanceCmd cmd)
        {
            processEngine.Activate(cmd);
            ActivateProcessInstanceResults cmdResult = new ActivateProcessInstanceResults(cmd.Id);
            commandResults.Send(MessageBuilder<ActivateProcessInstanceResults>.WithPayload(cmdResult).Build());
        }
    }
}