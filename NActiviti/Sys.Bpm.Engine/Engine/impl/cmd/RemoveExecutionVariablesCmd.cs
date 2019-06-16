using System;
using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.cmd
{

    
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.impl.util;

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