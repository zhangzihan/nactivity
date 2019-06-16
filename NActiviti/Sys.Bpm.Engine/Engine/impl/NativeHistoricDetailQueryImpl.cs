using System;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl
{

    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Interceptor;

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