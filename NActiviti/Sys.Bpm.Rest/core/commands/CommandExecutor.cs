using org.activiti.cloud.services.api.commands;
using System;

namespace org.activiti.cloud.services.core.commands
{
    /// <summary>
    /// 
    /// </summary>
    public interface CommandExecutor<T> where T : ICommand
    {
        /// <summary>
        /// 
        /// </summary>
        Type HandledType { get; }

        /// <summary>
        /// 
        /// </summary>
        void execute(T cmd);
    }
}