using System;

namespace org.activiti.engine.impl.jobexecutor
{
    using org.activiti.engine.impl.interceptor;

    public interface IFailedJobCommandFactory
    {

        ICommand<object> getCommand(string jobId, Exception exception);

    }

}