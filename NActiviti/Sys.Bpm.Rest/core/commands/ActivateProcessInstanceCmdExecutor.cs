using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Commands.Results;
using Sys.Workflow.Messaging;
using Sys.Workflow.Messaging.Support;
using System;

namespace Sys.Workflow.Cloud.Services.Core.Commands
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