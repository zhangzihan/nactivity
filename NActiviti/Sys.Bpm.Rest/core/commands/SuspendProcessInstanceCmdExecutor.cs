﻿using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Commands.Results;
using Sys.Workflow.Messaging;
using Sys.Workflow.Messaging.Support;
using System;

namespace Sys.Workflow.Cloud.Services.Core.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class SuspendProcessInstanceCmdExecutor : ICommandExecutor<SuspendProcessInstanceCmd>
    {

        private readonly ProcessEngineWrapper processEngine;
        private readonly IMessageChannel<SuspendProcessInstanceResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngine"></param>
        /// <param name="commandResults"></param>
        public SuspendProcessInstanceCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<SuspendProcessInstanceResults> commandResults)
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
                return typeof(SuspendProcessInstanceCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void Execute(SuspendProcessInstanceCmd cmd)
        {
            processEngine.Suspend(cmd);
            SuspendProcessInstanceResults cmdResult = new SuspendProcessInstanceResults(cmd.Id);
            commandResults.Send(MessageBuilder<SuspendProcessInstanceResults>.WithPayload(cmdResult).Build());
        }
    }
}