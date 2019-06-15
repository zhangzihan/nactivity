using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartSql;
using SmartSql.Abstractions;
using SmartSql.DbSession;
using Sys.Workflow.Options;
using System;
using System.IO;
using IDataSource = Sys.Data.IDataSource;

namespace Sys.Workflow
{
    /// <summary>
    /// 数据源依赖注入
    /// </summary>
    public static class ProcessEngineBuilderExtensionsDataSource
    {
        private static string DEFAULT_MYBATIS_MAPPING_FILE = "resources/db/mapping/mappings.xml";

        private static IServiceProvider serviceProvider;

        /// <summary>
        /// 注入数据源
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IProcessEngineBuilder AddDataSource(this IProcessEngineBuilder builder)
        {
            builder.AddSmartSqlDataSource()
                .AddSmartSqlMapper();

            return builder;
        }

        /// <summary>
        /// 注入SmartSql数据源
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        private static IProcessEngineBuilder AddSmartSqlDataSource(this IProcessEngineBuilder builder)
        {
            Action<IDataSource> updateSmartSqlDbSession = (dataSource) =>
            {
                ISmartSqlMapper ssm = serviceProvider.GetService<ISmartSqlMapper>();

                ssm.SmartSqlOptions.DbSessionStore = new DbConnectionSessionStore(serviceProvider.GetService<ILoggerFactory>(), dataSource.DbProviderFactory);

                ssm.SmartSqlOptions.SmartSqlContext.Database.WriteDataSource.ConnectionString = dataSource.ConnectionString;
                foreach (var ds in ssm.SmartSqlOptions.SmartSqlContext.Database.ReadDataSources)
                {
                    ds.ConnectionString = dataSource.ConnectionString;
                }
            };

            builder.Services.AddSingleton<IDataSource>(sp =>
            {
                serviceProvider = sp;

                return new Data.DataSource(sp.GetService<IOptionsMonitor<DataSourceOption>>(), updateSmartSqlDbSession);
            });

            return builder;
        }

        private static IProcessEngineBuilder AddSmartSqlMapper(this IProcessEngineBuilder builder)
        {
            builder.Services.AddSingleton<ISmartSqlMapper>(sp =>
            {
                var codebase = AppDomain.CurrentDomain.BaseDirectory;

                IDataSource dataSource = sp.GetService<IDataSource>();

                var dbSessionStore = new DbConnectionSessionStore(sp.GetService<ILoggerFactory>(), dataSource.DbProviderFactory);

                SmartSqlOptions options = new SmartSqlOptions
                {
                    ConfigPath = Path.Combine(codebase, DEFAULT_MYBATIS_MAPPING_FILE),
                    DbSessionStore = dbSessionStore
                };

                SmartSqlMapper ssm = new SmartSqlMapper(options);

                options.SmartSqlContext.Settings.IsWatchConfigFile = true;
                options.SmartSqlContext.Database.WriteDataSource.ConnectionString = dataSource.ConnectionString;
                foreach (var ds in options.SmartSqlContext.Database.ReadDataSources)
                {
                    ds.ConnectionString = dataSource.ConnectionString;
                }

                return ssm;
            });

            return builder;
        }
    }
}
