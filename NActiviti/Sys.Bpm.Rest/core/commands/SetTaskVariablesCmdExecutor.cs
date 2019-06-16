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
    public class SetTaskVariablesCmdExecutor : ICommandExecutor<SetTaskVariablesCmd>
    {

        private readonly ProcessEngineWrapper processEngine;
        private readonly IMessageChannel<SetTaskVariablesResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngine"></param>
        /// <param name="commandResults"></param>
        public SetTaskVariablesCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<SetTaskVariablesResults> commandResults)
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
                return typeof(SetTaskVariablesCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void Execute(SetTaskVariablesCmd cmd)
        {
            processEngine.SetTaskVariables(cmd);
            SetTaskVariablesResults cmdResult = new SetTaskVariablesResults(cmd.Id);
            commandResults.Send(MessageBuilder<SetTaskVariablesResults>.WithPayload(cmdResult).Build());
        }
    }

}