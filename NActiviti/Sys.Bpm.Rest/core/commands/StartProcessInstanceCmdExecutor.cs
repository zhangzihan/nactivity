using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Commands.Results;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Messaging;
using Sys.Workflow.Messaging.Support;
using System;

namespace Sys.Workflow.Cloud.Services.Core.Commands
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
            if (processInstance is object)
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