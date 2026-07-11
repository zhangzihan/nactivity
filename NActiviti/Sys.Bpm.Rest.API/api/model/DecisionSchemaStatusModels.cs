using System.Collections.Generic;

namespace Sys.Workflow.Cloud.Services.Api.Model
{
    /// <summary>
    /// 描述规则扩展 schema 的运行状态快照。
    /// </summary>
    public sealed class DecisionSchemaStatusRecord
    {
        /// <summary>
        /// 状态标识。
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 当前工作流数据库类型。
        /// </summary>
        public string DatabaseType { get; set; } = string.Empty;

        /// <summary>
        /// 当前 schema 更新策略。
        /// </summary>
        public string DatabaseSchemaUpdate { get; set; } = string.Empty;

        /// <summary>
        /// 当前引擎期望的 schema 版本。
        /// </summary>
        public string ExpectedSchemaVersion { get; set; } = string.Empty;

        /// <summary>
        /// 数据库中当前记录的 schema 版本。
        /// </summary>
        public string ActualSchemaVersion { get; set; }

        /// <summary>
        /// 当前数据源是否为 PostgreSQL。
        /// </summary>
        public bool IsPostgreSql { get; set; }

        /// <summary>
        /// 当前数据源是否为 SQL Server。
        /// </summary>
        public bool IsSqlServer { get; set; }

        /// <summary>
        /// 是否实际执行了规则扩展 schema 检查。
        /// </summary>
        public bool DecisionSchemaChecked { get; set; }

        /// <summary>
        /// 核心 schema 版本是否一致。
        /// </summary>
        public bool SchemaVersionMatches { get; set; }

        /// <summary>
        /// 规则扩展 schema 是否完整健康。
        /// </summary>
        public bool DecisionSchemaHealthy { get; set; }

        /// <summary>
        /// 是否需要执行升级动作。
        /// </summary>
        public bool UpgradeRequired { get; set; }

        /// <summary>
        /// 缺失的规则扩展表集合。
        /// </summary>
        public IList<string> MissingTables { get; set; } = new List<string>();

        /// <summary>
        /// 启动日志统一告警前缀。
        /// </summary>
        public string WarningPrefix { get; set; } = string.Empty;

        /// <summary>
        /// schema 版本落后时使用的告警事件号。
        /// </summary>
        public int VersionMismatchEventId { get; set; }

        /// <summary>
        /// 扩展表缺失时使用的告警事件号。
        /// </summary>
        public int MissingTablesEventId { get; set; }

        /// <summary>
        /// 推荐处理动作。
        /// </summary>
        public string RecommendedAction { get; set; } = string.Empty;

        /// <summary>
        /// 检查失败时的错误摘要。
        /// </summary>
        public string InspectionError { get; set; }
    }
}
