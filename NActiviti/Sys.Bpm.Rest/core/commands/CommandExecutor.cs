using org.activiti.cloud.services.api.commands;
using System;

namespace org.activiti.cloud.services.core.commands
{
    public interface CommandExecutor<T> where T : ICommand
    {
        Type HandledType { get; }

        void execute(T cmd);
    }
}