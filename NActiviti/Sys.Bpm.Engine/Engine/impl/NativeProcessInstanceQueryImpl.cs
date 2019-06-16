using System;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl
{

    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Runtime;

    [Serializable]
    public class NativeProcessInstanceQueryImpl : AbstractNativeQuery<INativeProcessInstanceQuery, IProcessInstance>, INativeProcessInstanceQuery
    {

        private const long serialVersionUID = 1L;

        public NativeProcessInstanceQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public NativeProcessInstanceQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        // results ////////////////////////////////////////////////////////////////

        public override IList<IProcessInstance> ExecuteList(ICommandContext commandContext, IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return commandContext.ExecutionEntityManager.FindProcessInstanceByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public override long ExecuteCount(ICommandContext commandContext, IDictionary<string, object> parameterMap)
        {
            return commandContext.ExecutionEntityManager.FindExecutionCountByNativeQuery(parameterMap);
            // can use execution count, since the result type doesn't matter
        }

    }

}