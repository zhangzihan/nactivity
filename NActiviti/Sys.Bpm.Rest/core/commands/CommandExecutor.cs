using org.activiti.cloud.services.api.commands;
using System;

namespace org.activiti.cloud.services.core.commands
{
    public interface CommandExecutor<T> where T : Command
    {
        Type HandledType { get; }

        void execute(T cmd);
    }
}