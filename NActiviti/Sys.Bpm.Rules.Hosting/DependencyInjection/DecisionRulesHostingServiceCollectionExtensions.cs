using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sys.Workflow.Options;
using Sys.Workflow.Rules;
using Sys.Workflow.Validation;
using Sys.Workflow.Validation.Validators;
using Sys.Workflow.Validation.Validators.Impl;

namespace Sys.Workflow
{
    /// <summary>
    /// 提供规则宿主装配与默认聚合入口。
    /// </summary>
    public static class DecisionRulesHostingServiceCollectionExtensions
    {
        /// <summary>
        /// 以默认 WorkflowDataSource 配置装配流程引擎与规则宿主。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <param name="configuration">应用配置。</param>
        /// <returns>流程引擎构建器。</returns>
        public static IProcessEngineBuilder AddProcessEngineWithRules(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddProcessEngineWithRules(configuration, option =>
            {
                IConfiguration config = configuration.GetSection("WorkflowDataSource");
                config.Bind(option);
            });
        }

        /// <summary>
        /// 装配流程引擎并追加规则宿主模块。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <param name="configuration">应用配置。</param>
        /// <param name="setupAction">工作流数据源配置委托。</param>
        /// <returns>流程引擎构建器。</returns>
        public static IProcessEngineBuilder AddProcessEngineWithRules(this IServiceCollection services, IConfiguration configuration, Action<DataSourceOption> setupAction)
        {
            IProcessEngineBuilder builder = services.AddProcessEngine(configuration, setupAction);
            return builder.AddDecisionRulesHosting(configuration, setupAction);
        }

        /// <summary>
        /// 在现有流程引擎构建器上追加规则宿主装配。
        /// </summary>
        /// <param name="builder">流程引擎构建器。</param>
        /// <param name="configuration">应用配置。</param>
        /// <param name="setupAction">工作流数据源配置委托。</param>
        /// <returns>流程引擎构建器。</returns>
        public static IProcessEngineBuilder AddDecisionRulesHosting(this IProcessEngineBuilder builder, IConfiguration configuration, Action<DataSourceOption> setupAction)
        {
            AddDecisionRulesInfrastructure(builder, configuration, setupAction);
            ReplaceProcessValidator(builder);
            return builder;
        }

        /// <summary>
        /// 注入规则仓储与 DMN 决策运行时。
        /// </summary>
        /// <param name="builder">流程引擎构建器。</param>
        /// <param name="configuration">应用配置。</param>
        /// <param name="setupAction">工作流数据源配置委托。</param>
        private static void AddDecisionRulesInfrastructure(IProcessEngineBuilder builder, IConfiguration configuration, Action<DataSourceOption> setupAction)
        {
            DataSourceOption workflowDataSourceOption = ResolveWorkflowDataSourceOption(configuration, setupAction);
            IConfigurationSection decisionRulesStoreSection = configuration.GetSection("DecisionRulesStore");

            if (decisionRulesStoreSection.Exists())
            {
                builder.Services.AddDecisionRulesStore(options =>
                {
                    decisionRulesStoreSection.Bind(options);
                    if (string.IsNullOrWhiteSpace(options.DatabaseType))
                    {
                        options.DatabaseType = ResolveDecisionRulesStoreDatabaseType(workflowDataSourceOption);
                    }

                    if (string.IsNullOrWhiteSpace(options.ConnectionString))
                    {
                        options.ConnectionString = workflowDataSourceOption.ConnectionString;
                    }
                });
            }
            else if (IsSupportedDecisionRulesDataSource(workflowDataSourceOption))
            {
                builder.Services.AddDecisionRulesStore(options =>
                {
                    options.DatabaseType = ResolveDecisionRulesStoreDatabaseType(workflowDataSourceOption);
                    options.ConnectionString = workflowDataSourceOption.ConnectionString;
                });
            }

            builder.Services.AddDmnDecisionExecutionService();
        }

        /// <summary>
        /// 用带目录校验的流程校验器替换纯 engine 默认校验器。
        /// </summary>
        /// <param name="builder">流程引擎构建器。</param>
        private static void ReplaceProcessValidator(IProcessEngineBuilder builder)
        {
            builder.Services.RemoveAll<IProcessValidator>();
            builder.Services.AddSingleton<IProcessValidator>(sp => CreateRulesHostingProcessValidator(sp));
        }

        /// <summary>
        /// 创建包含规则目录校验的流程校验器。
        /// </summary>
        /// <param name="serviceProvider">应用服务提供器。</param>
        /// <returns>流程校验器。</returns>
        private static IProcessValidator CreateRulesHostingProcessValidator(IServiceProvider serviceProvider)
        {
            IProcessValidator processValidator = ProcessEngineBuilderExtensionsProcessEngine.CreateDefaultProcessValidator();
            if (processValidator is ProcessValidatorImpl processValidatorImpl)
            {
                var validatorSet = new ValidatorSet("workflow-decision-catalog");
                validatorSet.AddValidator(new DecisionBindingCatalogValidator(serviceProvider.GetService<IDecisionCatalogService>()));
                processValidatorImpl.AddValidatorSet(validatorSet);
            }

            return processValidator;
        }

        /// <summary>
        /// 解析当前工作流数据源配置。
        /// </summary>
        /// <param name="configuration">应用配置。</param>
        /// <param name="setupAction">工作流数据源配置委托。</param>
        /// <returns>当前工作流数据源配置。</returns>
        private static DataSourceOption ResolveWorkflowDataSourceOption(IConfiguration configuration, Action<DataSourceOption> setupAction)
        {
            var dataSourceOption = new DataSourceOption();
            configuration.GetSection("WorkflowDataSource").Bind(dataSourceOption);
            setupAction(dataSourceOption);
            return dataSourceOption;
        }

        /// <summary>
        /// 判断当前工作流数据源是否为 PostgreSQL。
        /// </summary>
        /// <param name="dataSourceOption">数据源配置。</param>
        /// <returns>是否为 PostgreSQL 数据源。</returns>
        private static bool IsPostgreSqlDataSource(DataSourceOption dataSourceOption)
        {
            if (dataSourceOption == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(dataSourceOption.ProviderName) &&
                (dataSourceOption.ProviderName.Contains("npgsql", StringComparison.OrdinalIgnoreCase) ||
                 dataSourceOption.ProviderName.Contains("postgres", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(dataSourceOption.ConnectionString) &&
                   dataSourceOption.ConnectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断当前工作流数据源是否为 SQL Server。
        /// </summary>
        /// <param name="dataSourceOption">数据源配置。</param>
        /// <returns>是否为 SQL Server 数据源。</returns>
        private static bool IsSqlServerDataSource(DataSourceOption dataSourceOption)
        {
            if (dataSourceOption == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(dataSourceOption.ProviderName) &&
                (dataSourceOption.ProviderName.Contains("sqlclient", StringComparison.OrdinalIgnoreCase) ||
                 dataSourceOption.ProviderName.Contains("mssql", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(dataSourceOption.ConnectionString) &&
                   dataSourceOption.ConnectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断当前数据源是否支持规则仓储持久化。
        /// </summary>
        /// <param name="dataSourceOption">数据源配置。</param>
        /// <returns>是否支持规则仓储。</returns>
        private static bool IsSupportedDecisionRulesDataSource(DataSourceOption dataSourceOption)
        {
            return IsPostgreSqlDataSource(dataSourceOption) || IsSqlServerDataSource(dataSourceOption);
        }

        /// <summary>
        /// 解析规则仓储使用的数据库类型标识。
        /// </summary>
        /// <param name="dataSourceOption">数据源配置。</param>
        /// <returns>数据库类型标识。</returns>
        private static string ResolveDecisionRulesStoreDatabaseType(DataSourceOption dataSourceOption)
        {
            if (IsSqlServerDataSource(dataSourceOption))
            {
                return "mssql";
            }

            if (IsPostgreSqlDataSource(dataSourceOption))
            {
                return "postgres";
            }

            return string.Empty;
        }
    }
}
