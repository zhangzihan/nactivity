using Microsoft.Extensions.Logging;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sys.Workflow.Cloud.Services.Core.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class CommandEndpoint
    {
        private readonly ILogger logger = null;

        private readonly IDictionary<Type, ICommandExecutor<ICommand>> commandExecutors;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdExecutors"></param>
        /// <param name="loggerFactory"></param>
        public CommandEndpoint(ISet<ICommandExecutor<ICommand>> cmdExecutors,
            ILoggerFactory loggerFactory)
        {
            this.commandExecutors = cmdExecutors.ToDictionary(x => x.GetType());//.ToDictionary(CommandExecutor::getHandledType, System.Func.identity());
            logger = loggerFactory.CreateLogger<CommandEndpoint>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void ConsumeActivateProcessInstanceCmd(ICommand cmd)
        {
            ProcessCommand(cmd);
        }

        private void ProcessCommand(ICommand cmd)
        {
            ICommandExecutor<ICommand> cmdExecutor = commandExecutors[cmd.GetType()];
            if (cmdExecutor is object)
            {
                cmdExecutor.Execute(cmd);
                return;
            }

            logger.LogDebug(">>> No Command Found for type: " + cmd.GetType());
        }
    }
}