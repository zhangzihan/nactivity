using Sys.Workflow.cloud.services.api.commands;
using System;

namespace Sys.Workflow.cloud.services.core.commands
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommandExecutor<T> where T : ICommand
    {
        /// <summary>
        /// 
        /// </summary>
        Type HandledType { get; }

        /// <summary>
        /// 
        /// </summary>
        void Execute(T cmd);
    }
}