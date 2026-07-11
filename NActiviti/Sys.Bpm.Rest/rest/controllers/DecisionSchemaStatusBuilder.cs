using System;
using System.Collections.Generic;
using System.Linq;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl.DB;

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{
    /// <summary>
    /// 构建规则扩展 schema 状态返回模型，支持 PostgreSQL 与 SQL Server。
    /// </summary>
    public static class DecisionSchemaStatusBuilder
    {
        /// <summary>
        /// 构建基础状态模型。
        /// </summary>
        /// <param name="databaseType">数据库类型。</param>
        /// <param name="databaseSchemaUpdate">schema 更新策略。</param>
        /// <returns>带公共字段的状态模型。</returns>
        public static DecisionSchemaStatusRecord CreateBaseStatus(string databaseType, string databaseSchemaUpdate)
        {
            return new DecisionSchemaStatusRecord
            {
                DatabaseType = databaseType ?? string.Empty,
                DatabaseSchemaUpdate = databaseSchemaUpdate ?? string.Empty,
                ExpectedSchemaVersion = ProcessEngineConstants.VERSION,
                WarningPrefix = DecisionSchemaWarningAdvisor.WarningPrefix,
                VersionMismatchEventId = DecisionSchemaWarningAdvisor.VersionMismatchEventId.Id,
                MissingTablesEventId = DecisionSchemaWarningAdvisor.MissingTablesEventId.Id
            };
        }

        /// <summary>
        /// 构建跳过检查的状态模型。
        /// </summary>
        /// <param name="databaseType">数据库类型。</param>
        /// <param name="databaseSchemaUpdate">schema 更新策略。</param>
        /// <returns>跳过检查的状态模型。</returns>
        public static DecisionSchemaStatusRecord BuildSkippedStatus(string databaseType, string databaseSchemaUpdate)
        {
            var status = CreateBaseStatus(databaseType, databaseSchemaUpdate);
            status.Status = "Skipped";
            status.IsPostgreSql = string.Equals(databaseType, DecisionSchemaWarningAdvisor.DatabaseTypePostgres, StringComparison.OrdinalIgnoreCase);
            status.IsSqlServer = string.Equals(databaseType, DecisionSchemaWarningAdvisor.DatabaseTypeSqlServer, StringComparison.OrdinalIgnoreCase);
            status.DecisionSchemaChecked = false;
            status.SchemaVersionMatches = false;
            status.DecisionSchemaHealthy = false;
            status.UpgradeRequired = false;
            string displayName = DecisionSchemaWarningAdvisor.ResolveDatabaseDisplayName(databaseType);
            status.RecommendedAction = $"当前工作流数据源为 {displayName}，不在规则扩展 schema 检查支持范围内，已跳过。";
            return status;
        }

        /// <summary>
        /// 构建检查失败的状态模型。
        /// </summary>
        /// <param name="databaseType">数据库类型。</param>
        /// <param name="databaseSchemaUpdate">schema 更新策略。</param>
        /// <param name="errorMessage">错误消息。</param>
        /// <returns>检查失败的状态模型。</returns>
        public static DecisionSchemaStatusRecord BuildInspectionFailedStatus(string databaseType, string databaseSchemaUpdate, string errorMessage)
        {
            var status = CreateBaseStatus(databaseType, databaseSchemaUpdate);
            status.Status = "InspectionFailed";
            status.IsPostgreSql = string.Equals(databaseType, DecisionSchemaWarningAdvisor.DatabaseTypePostgres, StringComparison.OrdinalIgnoreCase);
            status.IsSqlServer = string.Equals(databaseType, DecisionSchemaWarningAdvisor.DatabaseTypeSqlServer, StringComparison.OrdinalIgnoreCase);
            status.DecisionSchemaChecked = false;
            status.SchemaVersionMatches = false;
            status.DecisionSchemaHealthy = false;
            status.UpgradeRequired = false;
            status.InspectionError = errorMessage;
            status.RecommendedAction = "请检查工作流数据库连接、ACT_GE_PROPERTY 访问权限以及扩展表元数据读取能力。";
            return status;
        }

        /// <summary>
        /// 构建已检查数据库的规则扩展 schema 完整状态模型。
        /// </summary>
        /// <param name="databaseType">数据库类型标识。</param>
        /// <param name="databaseSchemaUpdate">schema 更新策略。</param>
        /// <param name="actualSchemaVersion">数据库中的 schema 版本。</param>
        /// <param name="missingTables">缺失的扩展表。</param>
        /// <returns>检查完成后的状态模型。</returns>
        public static DecisionSchemaStatusRecord BuildCheckedStatus(
            string databaseType,
            string databaseSchemaUpdate,
            string actualSchemaVersion,
            IReadOnlyCollection<string> missingTables)
        {
            var status = CreateBaseStatus(databaseType, databaseSchemaUpdate);
            string displayName = DecisionSchemaWarningAdvisor.ResolveDatabaseDisplayName(databaseType);
            string scriptPath = DecisionSchemaWarningAdvisor.ResolveUpgradeScriptPath(databaseType);

            status.Status = "Healthy";
            status.IsPostgreSql = string.Equals(databaseType, DecisionSchemaWarningAdvisor.DatabaseTypePostgres, StringComparison.OrdinalIgnoreCase);
            status.IsSqlServer = string.Equals(databaseType, DecisionSchemaWarningAdvisor.DatabaseTypeSqlServer, StringComparison.OrdinalIgnoreCase);
            status.DecisionSchemaChecked = true;
            status.ActualSchemaVersion = actualSchemaVersion;
            status.SchemaVersionMatches = string.Equals(
                actualSchemaVersion,
                ProcessEngineConstants.VERSION,
                StringComparison.OrdinalIgnoreCase);
            status.MissingTables = missingTables?
                .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();
            status.DecisionSchemaHealthy = status.SchemaVersionMatches && status.MissingTables.Count == 0;
            status.UpgradeRequired = !status.DecisionSchemaHealthy;

            if (status.DecisionSchemaHealthy)
            {
                status.RecommendedAction = $"当前 {displayName} 工作流 schema 与规则扩展表状态正常，无需额外处理。";
                return status;
            }

            status.Status = "UpgradeRequired";
            status.RecommendedAction = $"请执行 {scriptPath}，并确认 schema.version 与四张规则扩展表均已就绪。";
            return status;
        }
    }
}
