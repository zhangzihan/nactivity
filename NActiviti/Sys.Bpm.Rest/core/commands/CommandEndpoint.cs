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

    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class CommandEndpoint
    public class CommandEndpoint
    {
        private static readonly ILogger LOGGER = ProcessEngineServiceProvider.LoggerService<CommandEndpoint>();

        private IDictionary<Type, CommandExecutor<Command>> commandExecutors;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public CommandEndpoint(java.util.Set<CommandExecutor> cmdExecutors)
        public CommandEndpoint(ISet<CommandExecutor<Command>> cmdExecutors)
        {
            //JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            this.commandExecutors = cmdExecutors.ToDictionary(x => x.GetType());//.ToDictionary(CommandExecutor::getHandledType, System.Func.identity());
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @StreamListener(org.activiti.cloud.services.events.ProcessEngineChannels_Fields.COMMAND_CONSUMER) public void consumeActivateProcessInstanceCmd(org.activiti.cloud.services.api.commands.Command cmd)
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