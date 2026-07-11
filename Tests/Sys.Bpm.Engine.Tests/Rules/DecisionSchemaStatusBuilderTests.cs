using Sys.Workflow.Cloud.Services.Rest.Controllers;
using Xunit;

namespace Sys.Workflow.Rules.Tests
{
    /// <summary>
    /// 验证规则扩展 schema 状态构建器的输出口径。
    /// </summary>
    public class DecisionSchemaStatusBuilderTests
    {
        /// <summary>
        /// 验证不支持的数据库类型会返回跳过检查状态。
        /// </summary>
        [Fact]
        public void BuildSkippedStatus_Should_ReturnSkippedState()
        {
            var status = DecisionSchemaStatusBuilder.BuildSkippedStatus("oracle", "false");

            Assert.Equal("Skipped", status.Status);
            Assert.False(status.IsPostgreSql);
            Assert.False(status.IsSqlServer);
            Assert.False(status.DecisionSchemaChecked);
            Assert.Contains("跳过", status.RecommendedAction);
        }

        /// <summary>
        /// 验证 PostgreSQL schema 完整时返回健康状态。
        /// </summary>
        [Fact]
        public void BuildCheckedStatus_Should_ReturnHealthyState_WhenPostgreSqlSchemaIsComplete()
        {
            var status = DecisionSchemaStatusBuilder.BuildCheckedStatus(
                "postgres",
                "false",
                "7.0.0.1",
                new string[0]);

            Assert.Equal("Healthy", status.Status);
            Assert.True(status.IsPostgreSql);
            Assert.True(status.DecisionSchemaChecked);
            Assert.True(status.SchemaVersionMatches);
            Assert.True(status.DecisionSchemaHealthy);
            Assert.False(status.UpgradeRequired);
        }

        /// <summary>
        /// 验证 SQL Server schema 不完整时返回升级要求状态。
        /// </summary>
        [Fact]
        public void BuildCheckedStatus_Should_ReturnUpgradeRequiredState_WhenSqlServerSchemaIsIncomplete()
        {
            var status = DecisionSchemaStatusBuilder.BuildCheckedStatus(
                "mssql",
                "false",
                "7.0.0.0",
                new[] { "process_decision_reference" });

            Assert.Equal("UpgradeRequired", status.Status);
            Assert.False(status.IsPostgreSql);
            Assert.True(status.IsSqlServer);
            Assert.True(status.DecisionSchemaChecked);
            Assert.False(status.SchemaVersionMatches);
            Assert.False(status.DecisionSchemaHealthy);
            Assert.True(status.UpgradeRequired);
            Assert.Contains("SqlServer-规则决策表升级脚本-7000-to-7001.sql", status.RecommendedAction);
        }

        /// <summary>
        /// 验证检查失败时返回可观测错误摘要。
        /// </summary>
        [Fact]
        public void BuildInspectionFailedStatus_Should_ReturnInspectionFailureState()
        {
            var status = DecisionSchemaStatusBuilder.BuildInspectionFailedStatus("postgres", "false", "db offline");

            Assert.Equal("InspectionFailed", status.Status);
            Assert.True(status.IsPostgreSql);
            Assert.False(status.IsSqlServer);
            Assert.False(status.DecisionSchemaChecked);
            Assert.Equal("db offline", status.InspectionError);
        }
    }
}
