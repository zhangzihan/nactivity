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