using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sys.Workflow.Rules;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Sys.Workflow.DependencyInjection
{
    /// <summary>
    /// 定义流程引擎与规则决策装配的回归测试。
    /// </summary>
    public class ProcessEngineDecisionRulesRegistrationTests
    {
        /// <summary>
        /// 验证流程引擎默认会注册 DMN 运行时服务。
        /// </summary>
        [Fact]
        public void AddProcessEngine_Should_RegisterDecisionRuntimeService_WhenRulesStoreIsNotConfigured()
        {
            var services = new ServiceCollection();
            var configuration = BuildConfiguration(new Dictionary<string, string?>
            {
                ["WorkflowDataSource:providerName"] = "Microsoft.Data.SqlClient",
                ["WorkflowDataSource:database"] = "activiti",
                ["WorkflowDataSource:connectionString"] = "server=localhost;database=activiti;Integrated Security=True;"
            });

            services.AddLogging();
            services.AddProcessEngine(configuration);

            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDecisionRuntimeService));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDecisionDefinitionRepository));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDecisionVersionRepository));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDecisionExecutionLogRepository));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDecisionProcessReferenceIndexRepository));
        }

        /// <summary>
        /// 验证显式配置规则仓储时会一并注册版本仓储与执行日志仓储。
        /// </summary>
        [Fact]
        public void AddProcessEngine_Should_RegisterDecisionRepositories_WhenDecisionRulesStoreIsConfigured()
        {
            var services = new ServiceCollection();
            var configuration = BuildConfiguration(new Dictionary<string, string?>
            {
                ["WorkflowDataSource:providerName"] = "Microsoft.Data.SqlClient",
                ["WorkflowDataSource:database"] = "activiti",
                ["WorkflowDataSource:connectionString"] = "server=localhost;database=activiti;Integrated Security=True;",
                ["DecisionRulesStore:connectionString"] = "Server=localhost;Database=rulesdb;Integrated Security=True;",
                ["DecisionRulesStore:databaseType"] = "mssql",
                ["DecisionRulesStore:configPath"] = "resources/db/mapping/mssql/rules.mappings.xml"
            });

            services.AddLogging();
            services.AddProcessEngine(configuration);

            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDecisionRuntimeService));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDecisionVersionRepository));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDecisionExecutionLogRepository));
            Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDecisionGovernanceService));
        }

        /// <summary>
        /// 构造测试配置。
        /// </summary>
        /// <param name="values">配置键值对。</param>
        /// <returns>配置对象。</returns>
        private static IConfiguration BuildConfiguration(IDictionary<string, string?> values)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }
    }
}
