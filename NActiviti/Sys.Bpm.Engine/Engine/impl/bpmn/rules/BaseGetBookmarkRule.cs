using System;
using System.Collections.Generic;
using System.Text;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.Persistence.Entity;

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
