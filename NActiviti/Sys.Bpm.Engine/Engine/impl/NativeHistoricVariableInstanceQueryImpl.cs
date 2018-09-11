using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl
{

    using org.activiti.engine.history;
    using org.activiti.engine.impl.interceptor;

    [Serializable]
    public class NativeHistoricVariableInstanceQueryImpl : AbstractNativeQuery<INativeHistoricVariableInstanceQuery, IHistoricVariableInstance>, INativeHistoricVariableInstanceQuery
    {

        private const long serialVersionUID = 1L;

        public NativeHistoricVariableInstanceQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public NativeHistoricVariableInstanceQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        // results ////////////////////////////////////////////////////////////////

        public override IList<IHistoricVariableInstance> executeList(ICommandContext commandContext, IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return commandContext.HistoricVariableInstanceEntityManager.findHistoricVariableInstancesByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public override long executeCount(ICommandContext commandContext, IDictionary<string, object> parameterMap)
        {
            return commandContext.HistoricVariableInstanceEntityManager.findHistoricVariableInstanceCountByNativeQuery(parameterMap);
        }

    }
}