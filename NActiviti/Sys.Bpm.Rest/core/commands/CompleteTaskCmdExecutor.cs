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
    public class CompleteTaskCmdExecutor : ICommandExecutor<CompleteTaskCmd>
    {

        private readonly ProcessEngineWrapper processEngine;
        private readonly IMessageChannel<CompleteTaskResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        public CompleteTaskCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<CompleteTaskResults> commandResults)
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
                return typeof(CompleteTaskCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Execute(CompleteTaskCmd cmd)
        {
            processEngine.CompleteTask(cmd);
            CompleteTaskResults cmdResult = new CompleteTaskResults(cmd.Id);
            commandResults.Send(MessageBuilder<CompleteTaskResults>.WithPayload(cmdResult).Build());
        }
    }

}