using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.engine.impl.persistence.entity;
using System.Collections.Generic;

namespace Sys.Workflow.cloud.services.api.model
{

    /// <summary>
    /// 流程定义查询对象
    /// </summary>
    public class ProcessDefinitionQuery : AbstractQuery
    {

        /// <summary>
        /// 
        /// </summary>
        public ProcessDefinitionQuery()
        {

        }

        /// <summary>
        /// ids
        /// </summary>

        public string[] Ids { get; set; }

        /// <summary>
        /// 目录
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 包含目录
        /// </summary>
        public string CategoryLike { get; set; }

        /// <summary>
        /// 不等于目录
        /// </summary>
        public string CategoryNotEquals { get; set; }

        /// <summary>
        /// 部署id
        /// </summary>
        public string DeploymentId { get; set; }

        /// <summary>
        /// 部署ids
        /// </summary>
        public string[] DeploymentIds { get; set; }

        /// <summary>
        /// 流程键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 业务键 
        /// </summary>
        public string BusinessKey { get; set; }

        /// <summary>
        /// 业务路径
        /// </summary>
        public string BusinessPath { get; set; }

        /// <summary>
        /// 启动表单
        /// </summary>
        public string StartForm { get; set; }

        /// <summary>
        /// 包含键值
        /// </summary>
        public string KeyLike { get; set; }

        /// <summary>
        /// 键值keys
        /// </summary>
        public string[] Keys { get; set; }

        /// <summary>
        /// 资源名称
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// 包含资源名称
        /// </summary>
        public string ResourceNameLike { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public int? Version { get; set; }

        /// <summary>
        /// 大于版本
        /// </summary>
        public int? VersionGt { get; set; }

        /// <summary>
        /// 大于等于版本
        /// </summary>
        public int? VersionGte { get; set; }

        /// <summary>
        /// 小于版本
        /// </summary>
        public int? VersionLt { get; set; }

        /// <summary>
        /// 小于等于版本
        /// </summary>
        public int? VersionLte { get; set; }

        /// <summary>
        /// 最终版本
        /// </summary>
        public bool Latest { get; set; }

        /// <summary>
        /// 流程状态
        /// </summary>
        public ISuspensionState SuspensionState { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public string AuthorizationUserId { get; set; }

        /// <summary>
        /// 流程定义id
        /// </summary>
        public string ProcDefId { get; set; }

        /// <summary>
        /// 不使用租户id
        /// </summary>
        public bool WithoutTenantId { get; set; }
    }
}
