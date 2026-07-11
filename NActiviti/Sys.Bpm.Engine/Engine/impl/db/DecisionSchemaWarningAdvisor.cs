using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Sys.Workflow.Engine.Impl.DB
{
    /// <summary>
    /// 为规则决策扩展表提供启动期升级提示，支持 PostgreSQL 与 SQL Server。
    /// </summary>
    public static class DecisionSchemaWarningAdvisor
    {
        /// <summary>
        /// 启动期规则 schema 告警统一前缀。
        /// </summary>
        public const string WarningPrefix = "[decision-schema-warning]";

        /// <summary>
        /// schema 版本落后告警事件标识。
        /// </summary>
        public static readonly EventId VersionMismatchEventId = new(700001, "DecisionSchemaVersionMismatch");

        /// <summary>
        /// 扩展表缺失告警事件标识。
        /// </summary>
        public static readonly EventId MissingTablesEventId = new(700002, "DecisionSchemaMissingTables");

        /// <summary>
        /// PostgreSQL 数据库类型标识。
        /// </summary>
        public const string DatabaseTypePostgres = "postgres";

        /// <summary>
        /// SQL Server 数据库类型标识。
        /// </summary>
        public const string DatabaseTypeSqlServer = "mssql";

        private static readonly string[] RequiredTables =
        {
            "decision_definition",
            "decision_version",
            "decision_execution_log",
            "process_decision_reference"
        };

        /// <summary>
        /// 判断指定数据库类型是否支持规则扩展 schema 检查。
        /// </summary>
        /// <param name="databaseType">数据库类型标识。</param>
        /// <returns>是否支持规则扩展 schema 检查。</returns>
        public static bool IsSupportedDatabaseType(string databaseType)
        {
            if (string.IsNullOrWhiteSpace(databaseType))
            {
                return false;
            }

            return string.Equals(databaseType, DatabaseTypePostgres, StringComparison.OrdinalIgnoreCase)
                || string.Equals(databaseType, DatabaseTypeSqlServer, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 根据数据库类型解析对应的升级脚本路径。
        /// </summary>
        /// <param name="databaseType">数据库类型标识。</param>
        /// <returns>升级脚本路径；不支持时返回空。</returns>
        public static string ResolveUpgradeScriptPath(string databaseType)
        {
            if (string.Equals(databaseType, DatabaseTypePostgres, StringComparison.OrdinalIgnoreCase))
            {
                return "docs\\PostgreSQL-规则决策表升级脚本-7000-to-7001.sql";
            }

            if (string.Equals(databaseType, DatabaseTypeSqlServer, StringComparison.OrdinalIgnoreCase))
            {
                return "docs\\SqlServer-规则决策表升级脚本-7000-to-7001.sql";
            }

            return string.Empty;
        }

        /// <summary>
        /// 根据数据库类型解析对应的显示名称。
        /// </summary>
        /// <param name="databaseType">数据库类型标识。</param>
        /// <returns>数据库显示名称。</returns>
        public static string ResolveDatabaseDisplayName(string databaseType)
        {
            if (string.Equals(databaseType, DatabaseTypePostgres, StringComparison.OrdinalIgnoreCase))
            {
                return "PostgreSQL";
            }

            if (string.Equals(databaseType, DatabaseTypeSqlServer, StringComparison.OrdinalIgnoreCase))
            {
                return "SQL Server";
            }

            return databaseType ?? string.Empty;
        }

        /// <summary>
        /// 计算当前数据库缺失的规则决策扩展表。
        /// </summary>
        /// <param name="tableExists">用于判断表是否存在的委托。</param>
        /// <returns>缺失表名集合。</returns>
        public static IReadOnlyList<string> GetMissingTables(Func<string, bool> tableExists)
        {
            if (tableExists is null)
            {
                throw new ArgumentNullException(nameof(tableExists));
            }

            return RequiredTables
                .Where(tableName => !tableExists(tableName))
                .ToArray();
        }

        /// <summary>
        /// 生成版本不匹配时的手工升级告警文案。
        /// </summary>
        /// <param name="databaseType">数据库类型标识。</param>
        /// <param name="databaseVersion">数据库中的 schema 版本。</param>
        /// <param name="engineVersion">当前引擎期望版本。</param>
        /// <returns>可直接输出到日志的告警文案。</returns>
        public static string BuildVersionMismatchWarning(string databaseType, string databaseVersion, string engineVersion)
        {
            string displayName = ResolveDatabaseDisplayName(databaseType);
            string scriptPath = ResolveUpgradeScriptPath(databaseType);

            return $"{WarningPrefix} {displayName} workflow schema version is '{databaseVersion}', but the engine expects '{engineVersion}'. " +
                   "Because databaseSchemaUpdate=false, the 7000->7001 upgrade will not run automatically. " +
                   $"Run {scriptPath} before starting the host.";
        }

        /// <summary>
        /// 生成规则决策扩展表缺失时的手工升级告警文案。
        /// </summary>
        /// <param name="databaseType">数据库类型标识。</param>
        /// <param name="missingTables">缺失表名集合。</param>
        /// <param name="engineVersion">当前引擎期望版本。</param>
        /// <returns>可直接输出到日志的告警文案。</returns>
        public static string BuildMissingTablesWarning(string databaseType, IReadOnlyCollection<string> missingTables, string engineVersion)
        {
            if (missingTables is null)
            {
                throw new ArgumentNullException(nameof(missingTables));
            }

            string displayName = ResolveDatabaseDisplayName(databaseType);
            string tableList = string.Join(", ", missingTables.OrderBy(name => name, StringComparer.OrdinalIgnoreCase));
            string scriptPath = ResolveUpgradeScriptPath(databaseType);

            return $"{WarningPrefix} {displayName} decision schema is incomplete for engine version '{engineVersion}'. " +
                   $"Missing tables: {tableList}. " +
                   $"If the host runs with databaseSchemaUpdate=false, execute {scriptPath} to complete the 7000->7001 upgrade.";
        }
    }
}
