using Microsoft.Extensions.Logging;
using org.activiti.cloud.services.api.commands;
using Sys;
using System;
using System.Collections.Generic;
using System.Linq;

namespace org.activiti.cloud.services.core.commands
{
    //using Autowired = org.springframework.beans.factory.annotation.Autowired;
    //using StreamListener = org.springframework.cloud.stream.annotation.StreamListener;
    //using Component = org.springframework.stereotype.Component;

    public class CommandEndpoint
    {
        private static readonly ILogger LOGGER = ProcessEngineServiceProvider.LoggerService<CommandEndpoint>();

        private IDictionary<Type, CommandExecutor<Command>> commandExecutors;

        public CommandEndpoint(ISet<CommandExecutor<Command>> cmdExecutors)
        {
            this.commandExecutors = cmdExecutors.ToDictionary(x => x.GetType());//.ToDictionary(CommandExecutor::getHandledType, System.Func.identity());
        }

        public virtual void consumeActivateProcessInstanceCmd(Command cmd)
        {
            processCommand(cmd);
        }

        private void processCommand(Command cmd)
        {
            CommandExecutor<Command> cmdExecutor = commandExecutors[cmd.GetType()];
            if (cmdExecutor != null)
            {
                cmdExecutor.execute(cmd);
                return;
            }

            LOGGER.LogDebug(">>> No Command Found for type: " + cmd.GetType());
        }
    }
}