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
    public class RemoveProcessVariablesCmdExecutor : ICommandExecutor<RemoveProcessVariablesCmd>
    {

        private readonly ProcessEngineWrapper processEngine;
        private readonly IMessageChannel<RemoveProcessVariablesResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngine"></param>
        /// <param name="commandResults"></param>
        public RemoveProcessVariablesCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<RemoveProcessVariablesResults> commandResults)
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
                return typeof(RemoveProcessVariablesCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void Execute(RemoveProcessVariablesCmd cmd)
        {
            processEngine.RemoveProcessVariables(cmd);
            RemoveProcessVariablesResults cmdResult = new RemoveProcessVariablesResults(cmd.Id);
            commandResults.Send(MessageBuilder<RemoveProcessVariablesResults>.WithPayload(cmdResult).Build());
        }
    }
}