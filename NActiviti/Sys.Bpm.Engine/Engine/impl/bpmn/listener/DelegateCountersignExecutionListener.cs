using Microsoft.Extensions.Logging;
using org.activiti.engine.@delegate;
using org.activiti.engine.impl.persistence.entity;
using Sys;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.engine.impl.bpmn.listener
{
    public class DelegateCountersignExecutionListener : IExecutionListener
    {
        private readonly ILogger<DelegateCountersignExecutionListener> logger =
            ProcessEngineServiceProvider.LoggerService<DelegateCountersignExecutionListener>();

        public void notify(IExecutionEntity execution)
        {
            logger.LogDebug("contersing flow started...");

            execution.setVariable("userList", new string[] { "评审员", "主办方评审员" });
        }
    }
}
