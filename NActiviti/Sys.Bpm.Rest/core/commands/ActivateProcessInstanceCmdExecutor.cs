﻿using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.commands.results;
using org.springframework.messaging;
using org.springframework.messaging.support;
using System;

namespace org.activiti.cloud.services.core.commands
{
    /// <summary>
    /// 
    /// </summary>
    public class ActivateProcessInstanceCmdExecutor : CommandExecutor<ActivateProcessInstanceCmd>
    {

        private ProcessEngineWrapper processEngine;
        private IMessageChannel<ActivateProcessInstanceResults> commandResults;

        /// <summary>
        /// 
        /// </summary>
        public ActivateProcessInstanceCmdExecutor(ProcessEngineWrapper processEngine, IMessageChannel<ActivateProcessInstanceResults> commandResults)
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
                return typeof(ActivateProcessInstanceCmd);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void execute(ActivateProcessInstanceCmd cmd)
        {
            processEngine.activate(cmd);
            ActivateProcessInstanceResults cmdResult = new ActivateProcessInstanceResults(cmd.Id);
            commandResults.send(MessageBuilder<ActivateProcessInstanceResults>.withPayload(cmdResult).build());
        }
    }
}