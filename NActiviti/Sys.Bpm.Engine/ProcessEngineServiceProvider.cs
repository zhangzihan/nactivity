using DatabaseSchemaReader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using org.activiti.engine;
using org.activiti.engine.impl;
using org.activiti.engine.impl.agenda;
using org.activiti.engine.impl.asyncexecutor;
using org.activiti.engine.impl.cfg;
using org.activiti.engine.impl.util;
using SmartSql;
using SmartSql.Abstractions;
using SmartSql.Configuration;
using SmartSql.DbSession;
using Spring.Core;
using Spring.Core.TypeResolution;
using Sys.Bpm.Engine.impl;
using Sys.Data;
using System;
using System.Data.Common;
using System.IO;

namespace Sys
{
    public static class ProcessEngineServiceProvider
    {
        public static IServiceCollection AddProcessEngine(this IServiceCollection services)
        {
            TypeRegistry.RegisterType(typeof(CollectionUtil));

            services.AddTransient<IDataSource>(sp =>
            {
                var cfg = sp.GetService<IConfiguration>();

                var provider = cfg["SysDataSource:providerName"];
                var connStr = cfg["SysDataSource:connectionString"];

                return new Data.DataSource(provider, connStr);
            });

            services.AddSingleton<IDatabaseReader>(sp =>
             {
                 var ds = sp.GetService<IDataSource>();

                 return new DatabaseReader(ds.Connection as DbConnection);
             });

            services.AddSingleton<ISmartSqlMapper>(sp =>
            {
                var codebase = Path.GetDirectoryName(typeof(ProcessEngineConfiguration).Assembly.Location);

                IDataSource dataSource = sp.GetService<IDataSource>();

                var dbSessionStore = new DbConnectionSessionStore(sp.GetService<ILoggerFactory>(), dataSource.DbProviderFactory);

                SmartSqlOptions options = new SmartSqlOptions
                {
                    //LoggerFactory = sp.GetService<ILoggerFactory>(),
                    ConfigPath = Path.Combine(codebase, DEFAULT_MYBATIS_MAPPING_FILE),
                    DbSessionStore = dbSessionStore
                    //DataReaderDeserializerFactory = new DapperDataReaderDeserializerFactory()
                };

                SmartSqlMapper ssm = new SmartSqlMapper(options);

                options.SmartSqlContext.Database.WriteDataSource.ConnectionString = dataSource.ConnectionString;
                foreach (var ds in options.SmartSqlContext.Database.ReadDataSources)
                {
                    ds.ConnectionString = dataSource.ConnectionString;
                }

                return ssm;
            });

            services.AddTransient<IActivitiEngineAgendaFactory>(sp => new DefaultActivitiEngineAgendaFactory());

            services.AddSingleton<IIdGenerator>(sp => new GuidGenerator());

            services.AddSingleton<IBpmnParseFactory, DefaultBpmnParseFactory>();

            services.AddTransient<IAsyncExecutor>(sp =>
            {
                IConfigurationSection dajw = sp.GetService<IConfiguration>().GetSection("defaultAsyncJobAcquireWaitTimeInMillis");
                IConfigurationSection dtjw = sp.GetService<IConfiguration>().GetSection("defaultTimerJobAcquireWaitTimeInMillis");

                if (int.TryParse(dajw?.Value, out int iDajw) == false)
                {
                    iDajw = 1000;
                }
                if (int.TryParse(dtjw?.Value, out int iDtjw) == false)
                {
                    iDtjw = 1000;
                }

                return new DefaultAsyncJobExecutor()
                {
                    DefaultAsyncJobAcquireWaitTimeInMillis = iDajw,
                    DefaultTimerJobAcquireWaitTimeInMillis = iDtjw,
                };
            });

            services.AddTransient<ProcessEngineConfigurationImpl>(sp =>
            {
                ProcessEngineConfigurationImpl config = new StandaloneProcessEngineConfiguration(
                    new HistoryServiceImpl(),
                    new TaskServiceImpl(),
                    new DynamicBpmnServiceImpl(),
                    new RepositoryServiceImpl(),
                    new RuntimeServiceImpl(),
                    new ManagementServiceImpl(),
                    sp.GetService<IAsyncExecutor>(),
                    sp.GetService<IConfiguration>()
                );

                return config;
            });

            services.AddSingleton<ProcessEngineFactory>(sp =>
            {
                return ProcessEngineFactory.Instance;
            });

            services.AddTransient<IRepositoryService>(sp => sp.GetRequiredService<IProcessEngine>().RepositoryService);

            services.AddTransient<IRuntimeService>(sp => sp.GetRequiredService<IProcessEngine>().RuntimeService);

            services.AddTransient<IManagementService>(sp => sp.GetRequiredService<IProcessEngine>().ManagementService);

            services.AddTransient<IHistoryService>(sp => sp.GetRequiredService<IProcessEngine>().HistoryService);

            services.AddTransient<ITaskService>(sp => sp.GetRequiredService<IProcessEngine>().TaskService);

            services.AddTransient<IDynamicBpmnService>(sp => sp.GetRequiredService<IProcessEngine>().DynamicBpmnService);

            services.AddSingleton<IProcessEngine>(sp =>
            {
                return sp.GetService<ProcessEngineFactory>().DefaultProcessEngine;
            });

            ServiceProvider servicePprovider = services.BuildServiceProvider();
            servicePprovider.EnsureProcessEngineInit();

            services.AddSpringCoreService(servicePprovider.GetService<ILoggerFactory>());

            return services;
        }

        private static IServiceProvider serviceProvider;

        private static void EnsureProcessEngineInit(this IServiceProvider serviceProvider)
        {
            ProcessEngineServiceProvider.serviceProvider = serviceProvider;

            var engine = serviceProvider.GetService<IProcessEngine>();
            if (engine == null)
            {
                throw new InitProcessEngineFaliedException();
            }
        }

        public static T Resolve<T>()
        {
            return serviceProvider.GetService<T>();
        }

        private static string DEFAULT_MYBATIS_MAPPING_FILE = "resources/db/mapping/mappings.xml";

        public static ILogger<T> LoggerService<T>()
        {
            ILoggerFactory logFac = serviceProvider.GetService<ILoggerFactory>();
            if (logFac != null)
            {
                return logFac.CreateLogger<T>();
            }

            return new NoneLogger<T>();
        }
    }

    public class NoneLogger<T> : ILogger<T>, IDisposable
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Dispose()
        {
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {

        }
    }

    [System.Serializable]
    public sealed class InitProcessEngineFaliedException : Exception
    {
        public InitProcessEngineFaliedException()
        {
        }

        public InitProcessEngineFaliedException(string message) : base(message)
        {
        }

        public InitProcessEngineFaliedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
