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
    public class ClaimTaskCmdExecutor : ICommandExecutor<ClaimTaskCmd>
    {
        private readonly ProcessEngineWrapper processEngine;
        private readonly IMessageChannel<ClaimTaskResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        public ClaimTaskCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<ClaimTaskResults> commandResults)
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
                return typeof(ClaimTaskCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Execute(ClaimTaskCmd cmd)
        {
            processEngine.ClaimTask(cmd);
            ClaimTaskResults cmdResult = new ClaimTaskResults(cmd.Id);
            commandResults.Send(MessageBuilder<ClaimTaskResults>.WithPayload(cmdResult).Build());
        }
    }

}