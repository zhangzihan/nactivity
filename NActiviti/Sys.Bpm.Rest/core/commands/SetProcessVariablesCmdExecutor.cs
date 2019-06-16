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
    public class SetProcessVariablesCmdExecutor : ICommandExecutor<SetProcessVariablesCmd>
    {

        private readonly ProcessEngineWrapper processEngine;
        private readonly IMessageChannel<SetProcessVariablesResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngine"></param>
        /// <param name="commandResults"></param>
        public SetProcessVariablesCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<SetProcessVariablesResults> commandResults)
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
                return typeof(SetProcessVariablesCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void Execute(SetProcessVariablesCmd cmd)
        {
            processEngine.SetProcessVariables(cmd);
            SetProcessVariablesResults cmdResult = new SetProcessVariablesResults(cmd.Id);
            commandResults.Send(MessageBuilder<SetProcessVariablesResults>.WithPayload(cmdResult).Build());
        }
    }
}