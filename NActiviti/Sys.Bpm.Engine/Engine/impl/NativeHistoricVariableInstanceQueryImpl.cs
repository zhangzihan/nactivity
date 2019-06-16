using System;
using System.Collections.Generic;

namespace Sys.Workflow.engine.impl
{

    using Sys.Workflow.engine.history;
    using Sys.Workflow.engine.impl.interceptor;

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

        public override IList<IHistoricVariableInstance> ExecuteList(ICommandContext commandContext, IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return commandContext.HistoricVariableInstanceEntityManager.FindHistoricVariableInstancesByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public override long ExecuteCount(ICommandContext commandContext, IDictionary<string, object> parameterMap)
        {
            return commandContext.HistoricVariableInstanceEntityManager.FindHistoricVariableInstanceCountByNativeQuery(parameterMap);
        }

    }
}