using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spring.Core.TypeResolution;
using Spring.Extensions;
using Sys.Extentions;
using Sys.Net.Http;
using Sys.Workflow.Caches;
using Sys.Workflow.Engine.Bpmn.Rules;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Impl.Bpmn.Webservice;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.DB;
using Sys.Workflow.Engine.Impl.Util;
using Sys.Workflow.Options;
using Sys.Workflow.Util;
using System;
using System.Net.Http;
using Polly;
using System.Net;
using Polly.Extensions.Http;
using Sys.Workflow.Polly;
using Sys.Runtime.Serialization;
using Sys.Workflow.Engine.Impl.Scripting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sys.Workflow.Engine.Impl.EL;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl.Bpmn.Listeners;

namespace Sys.Workflow
{
	/// <summary>
	/// 流程服务依赖注入
	/// </summary>
	public static class ProcessEngineServiceCollectionExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public static IProcessEngineBuilder AddProcessEngine(this IServiceCollection services, IConfiguration configuration)
		{
			return services.AddProcessEngine(configuration, (option) =>
			{
				IConfiguration config = configuration.GetSection("WorkflowDataSource");

				config.Bind(option);
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		/// <param name="setupAction"></param>
		/// <returns></returns>
		public static IProcessEngineBuilder AddProcessEngine(this IServiceCollection services, IConfiguration configuration, Action<DataSourceOption> setupAction)
		{
			IProcessEngineBuilder builder = new ProcessEngineBuilder(services);

			return builder.AddProcessEngine(configuration, setupAction);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="configuration"></param>
		/// <param name="setupAction"></param>
		/// <returns></returns>
		public static IProcessEngineBuilder AddProcessEngine(this IProcessEngineBuilder builder, IConfiguration configuration, Action<DataSourceOption> setupAction)
		{
			ConfigUtil.Configuration = configuration;

			TypeRegistry.RegisterType(typeof(TimerUtil));
			TypeRegistry.RegisterType(typeof(ExternalConnector));
			builder.Services.AddSpringCoreTypeRepository();

			ILoggerFactory loggerFactory = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();

			builder.Services.AddSingleton<IUserDelegateAssignProxy, DefaultUserDelegateAssignProxy>();

			builder.Services.AddSpringCoreService(loggerFactory);

			builder.Services.AddSingleton<MemoryCacheProvider>();

			DbSqlSessionVersion.InitVersion(configuration);

			builder.Configure(configuration, setupAction);
			builder.AddDecisionRulesInfrastructure(configuration, setupAction);

			builder.Services.AddHttpClient("ExternalHttpClient")
				.AddRetryPolicy()
				.AddCircuitBreakerPolicy()
				.ConfigureHttpMessageHandlerBuilder(cb =>
				{
					if (cb.PrimaryHandler is HttpClientHandler handler)
					{
						handler.MaxRequestContentBufferSize = int.MaxValue;
						handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
						handler.UseDefaultCredentials = true;
						handler.ServerCertificateCustomValidationCallback += (s, arg1, arg2, arg3) => true;
					}

					//HttpClientHandler handler = new HttpClientHandler
					//{
					//    MaxRequestContentBufferSize = int.MaxValue,
					//    ClientCertificateOptions = ClientCertificateOption.Automatic,
					//    UseDefaultCredentials = true
					//};
					//handler.ServerCertificateCustomValidationCallback += (s, arg1, arg2, arg3) => true;

					//cb.PrimaryHandler = handler;
					//cb.Build();
				});

			builder.Services.AddTransient<IHttpClientProxy>(sp =>
			{
				try
				{
					HttpClient client = sp.GetService<IHttpClientFactory>().CreateClient("ExternalHttpClient");
					var url = sp.GetService<ExternalConnectorProvider>().WorkflowUrl;
					if (!string.IsNullOrWhiteSpace(url))
					{
						client.BaseAddress = new Uri(url);
					}

					return new DefaultHttpClientProxy(client,
						sp.GetService<IAccessTokenProvider>(),
						sp.GetService<IHttpContextAccessor>(),
						sp.GetService<ILoggerFactory>());
				}
				catch (Exception ex)
				{
					throw;
				}
			});

			var idGenerator = new GuidGenerator();
			builder.Services.AddSingleton<IIdGenerator>(sp => idGenerator);

			builder.Services.AddUserSession<DefaultUserSession>();

			builder.Services.AddWorkflowAccessTokenProvider<DefaultAccessTokenProvider>();

			builder.Services.AddTransient<IUserServiceProxy, DefaultUserServiceProxy>();

			builder.Services.AddTransient<IServiceWebApiHttpProxy, ServiceWebApiHttpProxy>();

			// 序列化器无状态、可共享，注册为单例避免每次解析都新建实例
			builder.Services.AddSingleton<ISerializableTypeSerializer, DefaultSerializableTypeSerializer>();

			builder.Services.TryAddSingleton<IExpressionFactory, ExpressionFactory>();
			builder.Services.TryAddSingleton<IScriptingEngines, ScriptingEngines>();
			builder.Services.TryAddSingleton<IScriptingEnginesProvider, ScriptingEnginesProvider>();
			builder.Services.AddSingleton<IProcessInstanceHelper, ProcessInstanceHelper>();

			builder.AddDataSource()
				.AddDataBaseReader()
				.AddAsyncExecutor()
				.AddProcessEngineConfiguration()
				.AddProcessEngineService();

			return builder;
		}

		/// <summary>
		/// 注入规则仓储与 DMN 决策运行时。
		/// </summary>
		/// <param name="builder">流程引擎构建器。</param>
		/// <param name="configuration">应用配置。</param>
		/// <param name="setupAction">工作流数据源配置委托。</param>
		private static void AddDecisionRulesInfrastructure(this IProcessEngineBuilder builder, IConfiguration configuration, Action<DataSourceOption> setupAction)
		{
			var workflowDataSourceOption = ResolveWorkflowDataSourceOption(configuration, setupAction);
			var decisionRulesStoreSection = configuration.GetSection("DecisionRulesStore");

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

			builder.Services.AddDmnDecisionRuntimeService();
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
