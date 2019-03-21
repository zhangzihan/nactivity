using Microsoft.Extensions.Logging;
using org.activiti.cloud.services.api.commands;
using Sys;
using System;
using System.Collections.Generic;
using System.Linq;

namespace org.activiti.cloud.services.core.commands
{
    /// <summary>
    /// 
    /// </summary>
    public class CommandEndpoint
    {
        private static readonly ILogger LOGGER = ProcessEngineServiceProvider.LoggerService<CommandEndpoint>();

        private IDictionary<Type, CommandExecutor<ICommand>> commandExecutors;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdExecutors"></param>
        public CommandEndpoint(ISet<CommandExecutor<ICommand>> cmdExecutors)
        {
            this.commandExecutors = cmdExecutors.ToDictionary(x => x.GetType());//.ToDictionary(CommandExecutor::getHandledType, System.Func.identity());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void consumeActivateProcessInstanceCmd(ICommand cmd)
        {
            processCommand(cmd);
        }

        private void processCommand(ICommand cmd)
        {
            CommandExecutor<ICommand> cmdExecutor = commandExecutors[cmd.GetType()];
            if (cmdExecutor != null)
            {
                cmdExecutor.execute(cmd);
                return;
            }

            LOGGER.LogDebug(">>> No Command Found for type: " + cmd.GetType());
        }
    }
}