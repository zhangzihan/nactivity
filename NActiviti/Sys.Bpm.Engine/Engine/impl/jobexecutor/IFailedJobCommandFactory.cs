using System;

namespace Sys.Workflow.engine.impl.jobexecutor
{
    using Sys.Workflow.engine.impl.interceptor;

    public interface IFailedJobCommandFactory
    {

        ICommand<object> GetCommand(string jobId, Exception exception);

    }

}