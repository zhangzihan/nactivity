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