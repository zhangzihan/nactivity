using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl.cmd
{

    
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;

    /// 
    /// 
    [Serializable]
    public class RemoveExecutionVariablesCmd : NeedsActiveExecutionCmd<object>
    {

        private const long serialVersionUID = 1L;

        private ICollection<string> variableNames;
        private bool isLocal;

        public RemoveExecutionVariablesCmd(string executionId, ICollection<string> variableNames, bool isLocal) : base(executionId)
        {
            this.variableNames = variableNames;
            this.isLocal = isLocal;
        }

        protected internal override object execute(ICommandContext commandContext, IExecutionEntity execution)
        {
            if (isLocal)
            {
                execution.removeVariablesLocal(variableNames);
            }
            else
            {
                execution.removeVariables(variableNames);
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