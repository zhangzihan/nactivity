using System.Collections.Generic;

namespace Sys.Workflow.Services.Api.Commands
{
    /// <summary>
    /// 启动流程命令
    /// </summary>
    public interface IStartProcessInstanceCmd
    {
        /// <summary>
        /// 业务键值
        /// </summary>
        string BusinessKey { get; set; }

        /// <summary>
        /// 流程定义id
        /// </summary>
        string ProcessDefinitionId { get; set; }

        /// <summary>
        /// 主流程关联的业务键值
        /// </summary>
        string ProcessDefinitionBusinessKey { get; set; }

        /// <summary>
        /// 主流程id
        /// </summary>
        string ProcessDefinitionKey { get; set; }

        /// <summary>
        /// 流程实例名
        /// </summary>
        string ProcessInstanceName { get; set; }

        /// <summary>
        /// 流程名称
        /// </summary>
        string ProcessName { get; set; }

        /// <summary>
        /// 启动表单
        /// </summary>
        string StartForm { get; set; }

        /// <summary>
        /// 租户id
        /// </summary>
        string TenantId { get; set; }

        /// <summary>
        /// 流程变量
        /// </summary>
        WorkflowVariable Variables { get; set; }

        /// <summary>
        /// 流程启动节点id
        /// </summary>
        string InitialFlowElementId { get; set; }

        /// <summary>
        /// 使用消息触发流程
        /// </summary>
        string StartByMessage { get; set; }
    }
}