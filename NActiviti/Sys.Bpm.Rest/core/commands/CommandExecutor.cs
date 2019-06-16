using Sys.Workflow.Cloud.Services.Api.Commands;
using System;

namespace Sys.Workflow.Cloud.Services.Core.Commands
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