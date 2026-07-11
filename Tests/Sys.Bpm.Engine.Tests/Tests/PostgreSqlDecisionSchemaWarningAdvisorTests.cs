using Sys.Workflow.Engine.Impl.DB;
using System;
using System.Collections.Generic;
using Xunit;

namespace Sys.Bpm.Engine.Tests.Tests
{
    /// <summary>
    /// 验证多数据库规则扩展 schema 告警助手的行为。
    /// </summary>
    public class DecisionSchemaWarningAdvisorTests
    {
        /// <summary>
        /// 验证缺表识别会返回 7000->7001 所需的未建表集合。
        /// </summary>
        [Fact]
        public void GetMissingTables_Should_ReturnMissingDecisionTables()
        {
            HashSet<string> existingTables = new(StringComparer.OrdinalIgnoreCase)
            {
                "decision_definition",
                "decision_version"
            };

            IReadOnlyList<string> missingTables = DecisionSchemaWarningAdvisor.GetMissingTables(existingTables.Contains);

            Assert.Equal(2, missingTables.Count);
            Assert.Contains("decision_execution_log", missingTables);
            Assert.Contains("process_decision_reference", missingTables);
        }

        /// <summary>
        /// 验证版本不匹配告警会包含手工升级脚本指引。
        /// </summary>
        [Fact]
        public void BuildVersionMismatchWarning_Should_ContainUpgradeScriptHint()
        {
            string warning = DecisionSchemaWarningAdvisor.BuildVersionMismatchWarning(
                DecisionSchemaWarningAdvisor.DatabaseTypePostgres,
                "7.0.0.0",
                "7.0.0.1");

            Assert.StartsWith(DecisionSchemaWarningAdvisor.WarningPrefix, warning, StringComparison.Ordinal);
            Assert.Contains("databaseSchemaUpdate=false", warning);
            Assert.Contains("7000->7001", warning);
            Assert.Contains("PostgreSQL-规则决策表升级脚本-7000-to-7001.sql", warning);
        }

        /// <summary>
        /// 验证缺表告警会列出缺失表名。
        /// </summary>
        [Fact]
        public void BuildMissingTablesWarning_Should_ListMissingTables()
        {
            string warning = DecisionSchemaWarningAdvisor.BuildMissingTablesWarning(
                DecisionSchemaWarningAdvisor.DatabaseTypeSqlServer,
                new[] { "process_decision_reference", "decision_execution_log" },
                "7.0.0.1");

            Assert.StartsWith(DecisionSchemaWarningAdvisor.WarningPrefix, warning, StringComparison.Ordinal);
            Assert.Contains("decision_execution_log", warning);
            Assert.Contains("process_decision_reference", warning);
            Assert.Contains("databaseSchemaUpdate=false", warning);
            Assert.Contains("SqlServer-规则决策表升级脚本-7000-to-7001.sql", warning);
        }

        /// <summary>
        /// 验证 schema 告警事件标识稳定可检索。
        /// </summary>
        [Fact]
        public void EventIds_Should_RemainStable()
        {
            Assert.Equal(700001, DecisionSchemaWarningAdvisor.VersionMismatchEventId.Id);
            Assert.Equal("DecisionSchemaVersionMismatch", DecisionSchemaWarningAdvisor.VersionMismatchEventId.Name);
            Assert.Equal(700002, DecisionSchemaWarningAdvisor.MissingTablesEventId.Id);
            Assert.Equal("DecisionSchemaMissingTables", DecisionSchemaWarningAdvisor.MissingTablesEventId.Name);
        }
    }
}
