using System.Collections.Generic;

namespace Sys.Workflow.Rules.Models
{
    /// <summary>
    /// 描述决策草稿保存请求。
    /// </summary>
    public sealed class DecisionDraftSaveRequest
    {
        /// <summary>
        /// 决策定义标识。
        /// </summary>
        public string? DecisionId { get; set; }

        /// <summary>
        /// 决策唯一键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 决策名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 决策分类。
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 决策描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 决策类型。
        /// </summary>
        public string? DecisionType { get; set; }

        /// <summary>
        /// 源格式。
        /// </summary>
        public string? SourceFormat { get; set; }

        /// <summary>
        /// 草稿版本标签。
        /// </summary>
        public string? VersionTag { get; set; }

        /// <summary>
        /// 决策定义内容。
        /// </summary>
        public string DefinitionContent { get; set; } = string.Empty;

        /// <summary>
        /// 标签集合。
        /// </summary>
        public IList<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// 定义摘要。
        /// </summary>
        public string? Checksum { get; set; }

        /// <summary>
        /// 保存人。
        /// </summary>
        public string? Operator { get; set; }
    }

    /// <summary>
    /// 描述决策版本发布请求。
    /// </summary>
    public sealed class PublishDecisionVersionRequest
    {
        /// <summary>
        /// 决策唯一键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 待发布版本标签；为空时优先取当前草稿版本。
        /// </summary>
        public string? VersionTag { get; set; }

        /// <summary>
        /// 发布说明。
        /// </summary>
        public string? PublishComment { get; set; }

        /// <summary>
        /// 发布人。
        /// </summary>
        public string? Operator { get; set; }
    }

    /// <summary>
    /// 描述决策版本回滚请求。
    /// </summary>
    public sealed class RollbackDecisionVersionRequest
    {
        /// <summary>
        /// 决策唯一键。
        /// </summary>
        public string DecisionKey { get; set; } = string.Empty;

        /// <summary>
        /// 目标回滚版本标签。
        /// </summary>
        public string TargetVersionTag { get; set; } = string.Empty;

        /// <summary>
        /// 回滚说明。
        /// </summary>
        public string? RollbackComment { get; set; }

        /// <summary>
        /// 操作人。
        /// </summary>
        public string? Operator { get; set; }
    }

    /// <summary>
    /// 描述草稿保存结果。
    /// </summary>
    public sealed class DecisionDraftSaveResult
    {
        /// <summary>
        /// 决策定义记录。
        /// </summary>
        public DecisionDefinitionRecord Definition { get; set; } = new DecisionDefinitionRecord();

        /// <summary>
        /// 草稿版本记录。
        /// </summary>
        public DecisionVersionRecord DraftVersion { get; set; } = new DecisionVersionRecord();
    }

    /// <summary>
    /// 描述版本发布或回滚结果。
    /// </summary>
    public sealed class DecisionVersionPublishResult
    {
        /// <summary>
        /// 决策定义记录。
        /// </summary>
        public DecisionDefinitionRecord Definition { get; set; } = new DecisionDefinitionRecord();

        /// <summary>
        /// 当前已发布版本记录。
        /// </summary>
        public DecisionVersionRecord PublishedVersion { get; set; } = new DecisionVersionRecord();
    }
}
