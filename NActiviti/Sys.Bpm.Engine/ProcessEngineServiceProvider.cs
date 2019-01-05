using DatabaseSchemaReader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using org.activiti.engine;
using org.activiti.engine.impl;
using org.activiti.engine.impl.agenda;
using org.activiti.engine.impl.cfg;
using org.activiti.engine.impl.util;
using SmartSql;
using SmartSql.Abstractions;
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

                return new DataSource(provider, connStr);
            });

            services.AddSingleton<IDatabaseReader>(sp =>
             {
                 var ds = sp.GetService<IDataSource>();

                 return new DatabaseReader(ds.Connection as DbConnection);
             });

            services.AddSingleton<ISmartSqlMapper>(sp =>
            {
                var codebase = Path.GetDirectoryName(typeof(ProcessEngineConfiguration).Assembly.Location);

                return new SmartSqlMapper(new SmartSqlOptions
                {
                    //LoggerFactory = sp.GetService<ILoggerFactory>(),
                    ConfigPath = Path.Combine(codebase, DEFAULT_MYBATIS_MAPPING_FILE),
                    //DataReaderDeserializerFactory = new DapperDataReaderDeserializerFactory()
                });
            });

            services.AddTransient<IRepositoryService>(sp => new RepositoryServiceImpl());

            services.AddTransient<IRuntimeService>(sp => new RuntimeServiceImpl());

            services.AddTransient<IManagementService>(sp => new ManagementServiceImpl());

            services.AddTransient<IHistoryService>(sp => new HistoryServiceImpl());

            services.AddTransient<ITaskService>(sp => new TaskServiceImpl());

            services.AddTransient<IDynamicBpmnService>(sp => new DynamicBpmnServiceImpl());

            services.AddTransient<IActivitiEngineAgendaFactory>(sp => new DefaultActivitiEngineAgendaFactory());

            services.AddSingleton<IIdGenerator>(sp => new GuidGenerator());

            services.AddSingleton<IBpmnParseFactory, DefaultBpmnParseFactory>();

            services.AddTransient<ProcessEngineConfiguration>(sp =>
            {
                ProcessEngineConfigurationImpl config = new StandaloneProcessEngineConfiguration(
                    sp.GetService<IHistoryService>(),
                    sp.GetService<ITaskService>(),
                    sp.GetService<IDynamicBpmnService>(),
                    sp.GetService<IRepositoryService>(),
                    sp.GetService<IRuntimeService>(),
                    sp.GetService<IManagementService>(),
                    sp.GetService<IConfiguration>()
                );

                return config;
            });

            services.AddSingleton<ProcessEngineFactory>(sp =>
            {
                return ProcessEngineFactory.Instance;
            });

            services.AddSingleton<IProcessEngine>(sp =>
            {
                return sp.GetService<ProcessEngineFactory>().DefaultProcessEngine;
            });

            return services;
        }

        private static IServiceProvider serviceProvider;

        public static void EnsureProcessEngineInit(this IServiceProvider serviceProvider)
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
