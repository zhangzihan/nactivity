namespace Sys.Workflow.Rules.Core.Options
{
    /// <summary>
    /// 定义决策仓储基础设施配置项。
    /// </summary>
    public sealed class DecisionRulesStoreOptions
    {
        /// <summary>
        /// SmartSql 配置文件路径。为空时按数据库类型选择默认配置。
        /// </summary>
        public string ConfigPath { get; set; } = string.Empty;

        /// <summary>
        /// 数据库连接字符串。
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// 数据库类型，支持 postgres 与 mssql。
        /// </summary>
        public string DatabaseType { get; set; } = string.Empty;

        /// <summary>
        /// 是否监听配置文件变更。
        /// </summary>
        public bool IsWatchConfigFile { get; set; }
    }
}
