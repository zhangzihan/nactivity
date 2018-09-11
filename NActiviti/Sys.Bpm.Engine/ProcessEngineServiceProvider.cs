using DatabaseSchemaReader;
using DryIoc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using org.activiti.engine;
using org.activiti.engine.impl;
using org.activiti.engine.impl.agenda;
using org.activiti.engine.impl.cfg;
using org.activiti.engine.impl.util;
using SmartSql;
using Sys.Bpm.Engine.impl;
using Sys.Data;
using System;
using System.Data.Common;
using System.IO;

namespace Sys
{
    public static class ProcessEngineServiceProvider
    {
        public static void AddProcessEngine(this IContainer container)
        {
            Spring.Core.TypeResolution.TypeRegistry.RegisterType(typeof(CollectionUtil));

            container.RegisterDelegate<IDataSource>(sp =>
            {
                var cfg = sp.Resolve<IConfiguration>();

                var provider = cfg["Database:Provider"];
                var connStr = cfg["Database:ConnectionString"];

                return new DataSource(provider, connStr);
            }, Reuse.Transient);

            container.RegisterDelegate<IDatabaseReader>(sp =>
            {
                var ds = sp.Resolve<IDataSource>();

                return new DatabaseReader(ds.Connection as DbConnection);
            }, Reuse.Singleton);

            container.RegisterDelegate<SmartSqlMapper>(sp =>
            {
                var codebase = Path.GetDirectoryName(typeof(ProcessEngineConfiguration).Assembly.Location);

                return new SmartSqlMapper(new SmartSqlOptions
                {
                    //LoggerFactory = sp.GetService<ILoggerFactory>(),
                    ConfigPath = Path.Combine(codebase, DEFAULT_MYBATIS_MAPPING_FILE),
                    //DataReaderDeserializerFactory = new DapperDataReaderDeserializerFactory()
                });
            }, Reuse.Singleton);

            container.RegisterDelegate<IRepositoryService>(sp => new RepositoryServiceImpl(), Reuse.Transient);

            container.RegisterDelegate<IRuntimeService>(sp => new RuntimeServiceImpl(), Reuse.Transient);

            container.RegisterDelegate<IManagementService>(sp => new ManagementServiceImpl(), Reuse.Transient);

            container.RegisterDelegate<IHistoryService>(sp => new HistoryServiceImpl(), Reuse.Transient);

            container.RegisterDelegate<ITaskService>(sp => new TaskServiceImpl(), Reuse.Transient);

            container.RegisterDelegate<IDynamicBpmnService>(sp => new DynamicBpmnServiceImpl(), Reuse.Transient);

            container.RegisterDelegate<IActivitiEngineAgendaFactory>(sp => new DefaultActivitiEngineAgendaFactory(), Reuse.Transient);

            container.RegisterDelegate<IIdGenerator>(sp => new GuidGenerator(), Reuse.Singleton);

            container.Register<IBpmnParseFactory, DefaultBpmnParseFactory>(Reuse.Singleton);

            container.RegisterDelegate<ProcessEngineConfiguration>(sp =>
            {
                ProcessEngineConfigurationImpl config = new StandaloneProcessEngineConfiguration(
                    sp.Resolve<IHistoryService>(),
                    sp.Resolve<ITaskService>(),
                    sp.Resolve<IDynamicBpmnService>(),
                    sp.Resolve<IRepositoryService>(),
                    sp.Resolve<IRuntimeService>(),
                    sp.Resolve<IManagementService>()
                );

                return config;
            }, Reuse.Transient);

            container.RegisterDelegate<ProcessEngineFactory>(sp =>
            {
                return ProcessEngineFactory.Instance;
            }, Reuse.Singleton);

            container.RegisterDelegate<IProcessEngine>(Span => DefaultProcessEngine, Reuse.Singleton);

            ServiceProvider = container;

            EnsureProcessEngineInit();
        }

        private static void EnsureProcessEngineInit()
        {
            if (DefaultProcessEngine == null)
            {
                throw new InitProcessEngineFaliedException();
            }
        }

        private static string DEFAULT_MYBATIS_MAPPING_FILE = "resources/db/mapping/mappings.xml";

        public static IContainer ServiceProvider { get; private set; }

        public static IConfiguration Configuration => ServiceProvider.Resolve<IConfiguration>();

        public static SmartSqlMapper SmartSqlMapper => ServiceProvider.Resolve<SmartSqlMapper>();

        public static IProcessEngine DefaultProcessEngine
        {
            get
            {
                var pef = ServiceProvider.Resolve<ProcessEngineFactory>();

                return pef.DefaultProcessEngine;
            }
        }

        public static ILogger<T> LoggerService<T>()
        {
            return new Logger<T>(new LoggerFactory());// NoneLogger<T>();// ServiceProvider.Resolve<ILogger<T>>();
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

    [Serializable]
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
