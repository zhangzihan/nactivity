using System;

namespace org.activiti.engine.impl.jobexecutor
{
    using org.activiti.engine.impl.interceptor;

    public interface IFailedJobCommandFactory
    {

        ICommand<object> GetCommand(string jobId, Exception exception);

    }

}