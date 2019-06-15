using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl
{

    using org.activiti.engine.history;
    using org.activiti.engine.impl.interceptor;

    [Serializable]
    public class NativeHistoricDetailQueryImpl : AbstractNativeQuery<INativeHistoricDetailQuery, IHistoricDetail>, INativeHistoricDetailQuery
    {

        private const long serialVersionUID = 1L;

        public NativeHistoricDetailQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public NativeHistoricDetailQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        // results ////////////////////////////////////////////////////////////////

        public override IList<IHistoricDetail> ExecuteList(ICommandContext commandContext, IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return commandContext.HistoricDetailEntityManager.FindHistoricDetailsByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public override long ExecuteCount(ICommandContext commandContext, IDictionary<string, object> parameterMap)
        {
            return commandContext.HistoricDetailEntityManager.FindHistoricDetailCountByNativeQuery(parameterMap);
        }

    }
}