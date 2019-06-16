using System;
using System.Collections.Generic;

namespace Sys.Workflow.engine.impl
{
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.repository;


    [Serializable]
    public class NativeDeploymentQueryImpl : AbstractNativeQuery<INativeDeploymentQuery, IDeployment>, INativeDeploymentQuery
    {

        private const long serialVersionUID = 1L;

        public NativeDeploymentQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public NativeDeploymentQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        // results ////////////////////////////////////////////////////////////////

        public override IList<IDeployment> ExecuteList(ICommandContext commandContext, IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return commandContext.DeploymentEntityManager.FindDeploymentsByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public override long ExecuteCount(ICommandContext commandContext, IDictionary<string, object> parameterMap)
        {
            return commandContext.DeploymentEntityManager.FindDeploymentCountByNativeQuery(parameterMap);
        }

    }

}