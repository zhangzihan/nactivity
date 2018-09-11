using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.runtime;

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

        public override IList<IProcessInstance> executeList(ICommandContext commandContext, IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return commandContext.ExecutionEntityManager.findProcessInstanceByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public override long executeCount(ICommandContext commandContext, IDictionary<string, object> parameterMap)
        {
            return commandContext.ExecutionEntityManager.findExecutionCountByNativeQuery(parameterMap);
            // can use execution count, since the result type doesn't matter
        }

    }

}