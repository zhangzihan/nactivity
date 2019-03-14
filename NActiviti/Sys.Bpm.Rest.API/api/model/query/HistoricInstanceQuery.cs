using org.activiti.api.runtime.shared.query;
using org.activiti.engine.impl.persistence.entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.cloud.services.api.model
{
    /// <summary>
    /// 流程实例查询条件对象
    /// </summary>
    public class HistoricInstanceQuery : AbstractQuery
    {
        /// <summary>
        /// 业务键
        /// </summary>
        public string BusinessKey { get; set; }

        /// <summary>
        /// 流程定义id
        /// </summary>
        public string ProcessDefinitionId { get; set; }

        /// <summary>
        /// 多个流程定义id
        /// </summary>
        public string[] ProcessDefinitionIds { get; set; }

        /// <summary>
        /// 流程定义业务键
        /// </summary>
        public string ProcessDefinitionKey { get; set; }

        /// <summary>
        /// 流程定义业务键列表
        /// </summary>
        public string[] ProcessDefinitionKeys { get; set; }

        /// <summary>
        /// 流程定义名称
        /// </summary>
        public string ProcessDefinitionName { get; set; }

        /// <summary>
        /// 流程定义版本号
        /// </summary>
        public int? ProcessDefinitionVersion { get; set; }

        /// <summary>
        /// 流程实例id
        /// </summary>
        public string ProcessInstanceId { get; set; }

        /// <summary>
        /// 流程实例id列表
        /// </summary>
        public string[] ProcessInstanceIds { get; set; }

        /// <summary>
        /// 在某个时间之后开始
        /// </summary>
        public DateTime? FinishedAfter { get; set; }

        /// <summary>
        /// 在某个时间之前开始
        /// </summary>
        public DateTime? FinishedBefore { get; set; }

        /// <summary>
        /// 在某个时间之后开始
        /// </summary>
        public DateTime? StartedAfter { get; set; }

        /// <summary>
        /// 在某个时间之前开始
        /// </summary>
        public DateTime? StartedBefore { get; set; }

        /// <summary>
        /// 启动用户
        /// </summary>
        public string StartedBy { get; set; }

        /// <summary>
        /// 是否不使用租户id
        /// </summary>
        public bool? WithoutTenantId { get; set; } = false;
    }
}
