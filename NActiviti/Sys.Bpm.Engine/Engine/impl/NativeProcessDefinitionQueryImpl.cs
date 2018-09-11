using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl
{
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.repository;


    [Serializable]
    public class NativeProcessDefinitionQueryImpl : AbstractNativeQuery<INativeProcessDefinitionQuery, IProcessDefinition>, INativeProcessDefinitionQuery
    {

        private const long serialVersionUID = 1L;

        public NativeProcessDefinitionQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public NativeProcessDefinitionQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        // results ////////////////////////////////////////////////////////////////

        public override IList<IProcessDefinition> executeList(ICommandContext commandContext, IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return commandContext.ProcessDefinitionEntityManager.findProcessDefinitionsByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public override long executeCount(ICommandContext commandContext, IDictionary<string, object> parameterMap)
        {
            return commandContext.ProcessDefinitionEntityManager.findProcessDefinitionCountByNativeQuery(parameterMap);
        }

    }

}