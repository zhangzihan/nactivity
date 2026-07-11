using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using Npgsql;
using SmartSql;
using SmartSql.Abstractions;
using SmartSql.Abstractions.DbSession;
using SmartSql.DbSession;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Core.Options;
using Sys.Workflow.Rules.Core.Repositories;
using Sys.Workflow.Rules.Core.Services;
using Sys.Workflow.Rules.Models;
using System;
using System.IO;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 定义规则仓储基础设施的依赖注入扩展。
    /// </summary>
    public static class DecisionRulesServiceCollectionExtensions
    {
        private const string LegacyDefaultConfigPath = "resources/db/mapping/rules.mappings.xml";
        private const string DefaultPostgresConfigPath = "resources/db/mapping/postgres/rules.mappings.xml";
        private const string DefaultSqlServerConfigPath = "resources/db/mapping/mssql/rules.mappings.xml";
        private const string DatabaseTypePostgres = "postgres";
        private const string DatabaseTypeSqlServer = "mssql";

        /// <summary>
        /// 注册规则仓储基础设施。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <param name="setupAction">规则仓储配置委托。</param>
        /// <returns>服务集合。</returns>
        public static IServiceCollection AddDecisionRulesStore(this IServiceCollection services, Action<DecisionRulesStoreOptions> setupAction)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(setupAction);

            services.Configure(setupAction);
            return services.AddDecisionRulesStore();
        }

        /// <summary>
        /// 注册规则仓储基础设施。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>服务集合。</returns>
        public static IServiceCollection AddDecisionRulesStore(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddOptions<DecisionRulesStoreOptions>();
            services.AddSingleton<IDbConnectionSessionStore>(CreateDbConnectionSessionStore);
            services.AddSingleton<ISmartSqlMapper>(CreateSmartSqlMapper);
            services.AddSingleton<DecisionVersionRepository>();
            services.AddSingleton<DecisionDefinitionRepository>();
            services.AddSingleton<DecisionExecutionLogRepository>();
            services.AddSingleton<DecisionProcessReferenceIndexRepository>();
            services.AddSingleton<DecisionGovernanceService>(sp => new DecisionGovernanceService(
                sp.GetRequiredService<IDecisionDefinitionRepository>(),
                sp.GetRequiredService<IDecisionVersionRepository>(),
                sp.GetRequiredService<ISmartSqlMapper>()));
            services.AddSingleton<IDecisionVersionRepository>(sp => sp.GetRequiredService<DecisionVersionRepository>());
            services.AddSingleton<IDecisionDefinitionRepository>(sp => sp.GetRequiredService<DecisionDefinitionRepository>());
            services.AddSingleton<IDecisionExecutionLogRepository>(sp => sp.GetRequiredService<DecisionExecutionLogRepository>());
            services.AddSingleton<IDecisionProcessReferenceIndexRepository>(sp => sp.GetRequiredService<DecisionProcessReferenceIndexRepository>());
            services.AddSingleton<IDecisionGovernanceService>(sp => sp.GetRequiredService<DecisionGovernanceService>());

            return services;
        }

        /// <summary>
        /// 创建 SmartSql 连接会话存储。
        /// </summary>
        /// <param name="serviceProvider">服务提供器。</param>
        /// <returns>连接会话存储。</returns>
        private static IDbConnectionSessionStore CreateDbConnectionSessionStore(IServiceProvider serviceProvider)
        {
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
            var storeOptions = serviceProvider.GetRequiredService<IOptions<DecisionRulesStoreOptions>>().Value;
            return new DbConnectionSessionStore(loggerFactory, ResolveDbProviderFactory(storeOptions.DatabaseType));
        }

        /// <summary>
        /// 创建并初始化 SmartSql 映射器。
        /// </summary>
        /// <param name="serviceProvider">服务提供器。</param>
        /// <returns>SmartSql 映射器。</returns>
        private static ISmartSqlMapper CreateSmartSqlMapper(IServiceProvider serviceProvider)
        {
            var storeOptions = serviceProvider.GetRequiredService<IOptions<DecisionRulesStoreOptions>>().Value;
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
            var configPath = ResolveConfigPath(storeOptions.ConfigPath, storeOptions.DatabaseType);

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"未找到规则仓储 SmartSql 配置文件：{configPath}", configPath);
            }

            if (string.IsNullOrWhiteSpace(storeOptions.ConnectionString))
            {
                throw new InvalidOperationException("DecisionRulesStoreOptions.ConnectionString 未配置。");
            }

            var smartSqlOptions = new SmartSqlOptions
            {
                ConfigPath = configPath,
                DbSessionStore = serviceProvider.GetRequiredService<IDbConnectionSessionStore>(),
                LoggerFactory = loggerFactory,
                ServiceProvider = serviceProvider
            };

            var mapper = new SmartSqlMapper(smartSqlOptions);
            smartSqlOptions.SmartSqlContext.Settings.IsWatchConfigFile = storeOptions.IsWatchConfigFile;
            ApplyConnectionString(smartSqlOptions, storeOptions.ConnectionString);

            return mapper;
        }

        /// <summary>
        /// 解析 SmartSql 配置文件的绝对路径。
        /// </summary>
        /// <param name="configPath">配置路径。</param>
        /// <returns>绝对路径。</returns>
        private static string ResolveConfigPath(string? configPath, string? databaseType)
        {
            var resolvedPath = string.IsNullOrWhiteSpace(configPath) ||
                               string.Equals(configPath, LegacyDefaultConfigPath, StringComparison.OrdinalIgnoreCase)
                ? ResolveDefaultConfigPath(databaseType)
                : configPath;
            if (Path.IsPathRooted(resolvedPath))
            {
                return resolvedPath;
            }

            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resolvedPath));
        }

        /// <summary>
        /// 解析当前数据库类型对应的默认 SmartSql 配置路径。
        /// </summary>
        /// <param name="databaseType">数据库类型。</param>
        /// <returns>默认配置路径。</returns>
        private static string ResolveDefaultConfigPath(string? databaseType)
        {
            if (string.Equals(databaseType, DatabaseTypeSqlServer, StringComparison.OrdinalIgnoreCase))
            {
                return DefaultSqlServerConfigPath;
            }

            return DefaultPostgresConfigPath;
        }

        /// <summary>
        /// 解析当前数据库类型对应的 ADO.NET Provider。
        /// </summary>
        /// <param name="databaseType">数据库类型。</param>
        /// <returns>数据库 Provider 工厂。</returns>
        private static System.Data.Common.DbProviderFactory ResolveDbProviderFactory(string? databaseType)
        {
            if (string.Equals(databaseType, DatabaseTypeSqlServer, StringComparison.OrdinalIgnoreCase))
            {
                return SqlClientFactory.Instance;
            }

            return NpgsqlFactory.Instance;
        }

        /// <summary>
        /// 将连接字符串写回 SmartSql 的读写数据源配置。
        /// </summary>
        /// <param name="smartSqlOptions">SmartSql 配置。</param>
        /// <param name="connectionString">连接字符串。</param>
        private static void ApplyConnectionString(SmartSqlOptions smartSqlOptions, string connectionString)
        {
            smartSqlOptions.SmartSqlContext.Database.WriteDataSource.ConnectionString = connectionString;

            foreach (var dataSource in smartSqlOptions.SmartSqlContext.Database.ReadDataSources)
            {
                dataSource.ConnectionString = connectionString;
            }
        }
    }
}
