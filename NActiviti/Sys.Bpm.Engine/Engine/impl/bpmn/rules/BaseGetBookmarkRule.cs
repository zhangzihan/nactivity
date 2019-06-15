using System;
using System.Collections.Generic;
using System.Text;
using org.activiti.engine.impl.interceptor;
using org.activiti.engine.impl.persistence.entity;

namespace Sys.Workflow.Engine.Bpmn.Rules
{
    /// <inheritdoc />
    public abstract class BaseGetBookmarkRule : IGetBookmarkRule
    {
        /// <inheritdoc />
        public IExecutionEntity Execution { get; set; }

        /// <inheritdoc />
        public virtual QueryBookmark Condition { get; set; }

        /// <inheritdoc />
        public abstract IList<IUserInfo> Execute(ICommandContext commandContext);
    }
}
