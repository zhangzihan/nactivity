using System;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl
{
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Repository;


    [Serializable]
    public class NativeModelQueryImpl : AbstractNativeQuery<INativeModelQuery, IModel>, INativeModelQuery
    {

        private const long serialVersionUID = 1L;

        public NativeModelQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public NativeModelQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        // results ////////////////////////////////////////////////////////////////

        public override IList<IModel> ExecuteList(ICommandContext commandContext, IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return commandContext.ModelEntityManager.FindModelsByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public override long ExecuteCount(ICommandContext commandContext, IDictionary<string, object> parameterMap)
        {
            return commandContext.ModelEntityManager.FindModelCountByNativeQuery(parameterMap);
        }

    }

}