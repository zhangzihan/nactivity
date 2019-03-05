using org.activiti.api.runtime.shared.query;
using org.activiti.engine.impl.persistence.entity;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.model
{
    public class ProcessDefinitionQuery : AbstractQuery
    {
        public ProcessDefinitionQuery()
        {

        }

        public ISet<string> Ids { get; set; }
        public string Category { get; set; }
        public string CategoryLike { get; set; }
        public string CategoryNotEquals { get; set; }
        public string DeploymentId { get; set; }
        public ISet<string> DeploymentIds { get; set; }
        public string Key { get; set; }
        public string BusinessKey { get; set; }
        public string BusinessPath { get; set; }
        public string StartForm { get; set; }
        public string KeyLike { get; set; }
        public ISet<string> Keys { get; set; }
        public string ResourceName { get; set; }
        public string ResourceNameLike { get; set; }
        public int? Version { get; set; }
        public int? VersionGt { get; set; }
        public int? VersionGte { get; set; }
        public int? VersionLt { get; set; }
        public int? VersionLte { get; set; }
        public bool Latest { get; set; }
        public ISuspensionState SuspensionState { get; set; }
        public string AuthorizationUserId { get; set; }
        public string ProcDefId { get; set; }
        public bool WithoutTenantId { get; set; }
    }
}
