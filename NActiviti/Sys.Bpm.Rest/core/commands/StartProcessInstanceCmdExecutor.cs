using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.cloud.services.api.commands.results;
using Sys.Workflow.cloud.services.api.model;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace Sys.Workflow.cloud.services.core.commands
{
    /// <summary>
    /// 
    /// </summary>
    public class StartProcessInstanceCmdExecutor : CommandExecutor<StartProcessInstanceCmd>
    {
        private ProcessEngineWrapper processEngine;
        private IMessageChannel<StartProcessInstanceResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngine"></param>
        /// <param name="commandResults"></param>
        public StartProcessInstanceCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<StartProcessInstanceResults> commandResults)
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
                return typeof(StartProcessInstanceCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void execute(StartProcessInstanceCmd cmd)
        {
            ProcessInstance[] processInstance = processEngine.startProcess(new StartProcessInstanceCmd[] { cmd });
            if (processInstance != null)
            {
                StartProcessInstanceResults cmdResult = new StartProcessInstanceResults(cmd.Id, processInstance);
                commandResults.send(MessageBuilder<StartProcessInstanceResults>.withPayload(cmdResult).build());
            }
            else
            {
                throw new System.InvalidOperationException("Failed to start processInstance");
            }
        }
    }

}