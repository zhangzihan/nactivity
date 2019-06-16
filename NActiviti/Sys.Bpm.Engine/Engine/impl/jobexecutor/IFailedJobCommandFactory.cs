using System;

namespace Sys.Workflow.Engine.Impl.JobExecutors
{
    using Sys.Workflow.Engine.Impl.Interceptor;

    public interface IFailedJobCommandFactory
    {

        ICommand<object> GetCommand(string jobId, Exception exception);

    }

}