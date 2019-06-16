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
    public class RemoveTaskVariablesCmd : NeedsActiveTaskCmd<object>
    {

        private const long serialVersionUID = 1L;

        private readonly ICollection<string> variableNames;
        private readonly bool isLocal;

        public RemoveTaskVariablesCmd(string taskId, ICollection<string> variableNames, bool isLocal) : base(taskId)
        {
            this.variableNames = variableNames;
            this.isLocal = isLocal;
        }

        protected internal override object Execute(ICommandContext commandContext, ITaskEntity task)
        {
            if (isLocal)
            {
                task.RemoveVariablesLocal(variableNames);
            }
            else
            {
                task.RemoveVariables(variableNames);
            }

            return null;
        }

        protected internal override string SuspendedTaskException
        {
            get
            {
                return "Cannot remove variables from a suspended task.";
            }
        }

    }

}