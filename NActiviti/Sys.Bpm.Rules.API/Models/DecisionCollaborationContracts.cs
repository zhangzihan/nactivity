using System.Collections.Generic;

namespace Sys.Workflow.Rules.Models
{
    /// <summary>
    /// 描述规则决策能力的宿主协同契约。
    /// </summary>
    public sealed class DecisionCollaborationDescriptor
    {
        /// <summary>
        /// 决策工作区相对路由。
        /// </summary>
        public string WorkspaceRoute { get; set; } = string.Empty;

        /// <summary>
        /// BPMN 属性面板打开工作区时使用的查询参数约定。
        /// </summary>
        public string WorkspacePickerQueryKey { get; set; } = string.Empty;

        /// <summary>
        /// 工作区对外广播的事件通道名。
        /// </summary>
        public string WorkspaceChannelEventName { get; set; } = string.Empty;

        /// <summary>
        /// 工作区向宿主发送的消息类型集合。
        /// </summary>
        public IList<string> WorkspaceMessageTypes { get; set; } = new List<string>();

        /// <summary>
        /// BPMN 页面内部使用的总线事件。
        /// </summary>
        public IList<string> WorkspaceBusEvents { get; set; } = new List<string>();

        /// <summary>
        /// 决策治理 Rest 接口前缀。
        /// </summary>
        public string GovernanceApiBasePath { get; set; } = string.Empty;

        /// <summary>
        /// 流程部署与引用分析 Rest 接口前缀。
        /// </summary>
        public string ProcessDeployerApiBasePath { get; set; } = string.Empty;

        /// <summary>
        /// 默认能力开关。
        /// </summary>
        public IDictionary<string, bool> CapabilityFlags { get; set; } =
            new Dictionary<string, bool>();
    }
}
