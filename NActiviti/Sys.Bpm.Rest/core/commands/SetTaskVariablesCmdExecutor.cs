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
    public class SetTaskVariablesCmdExecutor : CommandExecutor<SetTaskVariablesCmd>
    {

        private ProcessEngineWrapper processEngine;
        private IMessageChannel<SetTaskVariablesResults> commandResults;

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
        public virtual void execute(SetTaskVariablesCmd cmd)
        {
            processEngine.TaskVariables = cmd;
            SetTaskVariablesResults cmdResult = new SetTaskVariablesResults(cmd.Id);
            commandResults.send(MessageBuilder<SetTaskVariablesResults>.withPayload(cmdResult).build());
        }
    }

}