using System;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Cmd
{

    
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;

    /// 
    /// 
    [Serializable]
    public class RemoveExecutionVariablesCmd : NeedsActiveExecutionCmd<object>
    {

        private const long serialVersionUID = 1L;

        private IEnumerable<string> variableNames;
        private bool isLocal;

        public RemoveExecutionVariablesCmd(string executionId, IEnumerable<string> variableNames, bool isLocal) : base(executionId)
        {
            this.variableNames = variableNames;
            this.isLocal = isLocal;
        }

        protected internal override object Execute(ICommandContext commandContext, IExecutionEntity execution)
        {
            if (isLocal)
            {
                execution.RemoveVariablesLocal(variableNames);
            }
            else
            {
                execution.RemoveVariables(variableNames);
            }

            return null;
        }

        protected internal override string SuspendedExceptionMessage
        {
            get
            {
                return "Cannot remove variables because execution '" + executionId + "' is suspended";
            }
        }

    }

}