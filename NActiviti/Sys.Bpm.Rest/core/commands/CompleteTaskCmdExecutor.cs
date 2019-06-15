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