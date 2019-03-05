using org.activiti.api.runtime.shared.query;
using org.activiti.engine.impl.persistence.entity;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.model
{
    public class DeploymentQuery : AbstractQuery
    {
        public DeploymentQuery()
        {

        }

        public ISet<string> Ids { get; set; }
        public string Category { get; set; }
        public string CategoryLike { get; set; }
        public string CategoryNotEquals { get; set; }
        public string Key { get; set; }
        public string KeyLike { get; set; }
        public ISet<string> Keys { get; set; }
        public string BusinessKey { get; set; }
        public bool WithoutTenantId { get; set; }
        public bool LatestDeployment { get; set; }
    }
}
