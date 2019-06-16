using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    /// <summary>
    /// 部署用户Task的一些附件行为
    /// </summary>
    public interface IDeployExecutionBehavior
    {
        /// <summary>
        /// 部署用户Task的一些附件行为
        /// </summary>
        /// <param name="resources"></param>
        void Deploy(IDictionary<string, IResourceEntity> resources);
    }
}
