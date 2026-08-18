using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using SmartSql.Abstractions;
using SmartSql.Abstractions.Cache;
using SmartSql.Abstractions.Command;
using SmartSql.Abstractions.Config;
using SmartSql.Abstractions.DataReaderDeserializer;
using SmartSql.Abstractions.DataSource;
using SmartSql.Abstractions.DbSession;
using SmartSql.Exceptions;
using SmartSql.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SmartSql
{
	public class SmartSqlMapper : ISmartSqlMapper
	{
		private readonly SmartSqlOptions _smartSqlOptions;
		public SmartSqlOptions SmartSqlOptions => _smartSqlOptions;

		private readonly ILogger _logger;
		public IDbConnectionSessionStore SessionStore => _smartSqlOptions.DbSessionStore;
		public IDataSourceFilter DataSourceFilter => _smartSqlOptions.DataSourceFilter;
		public IConfigLoader ConfigLoader => _smartSqlOptions.ConfigLoader;
		public ICommandExecuter CommandExecuter => _smartSqlOptions.CommandExecuter;
		public IDataReaderDeserializerFactory DeserializerFactory => _smartSqlOptions.DataReaderDeserializerFactory;
		public ILoggerFactory LoggerFactory => _smartSqlOptions.LoggerFactory;
		public ICacheManager CacheManager => _smartSqlOptions.CacheManager;
		public ISqlBuilder SqlBuilder => _smartSqlOptions.SqlBuilder;

		private JToken variables;
		public JToken Variables
		{
			get => variables;
			set => variables = value ?? throw new ArgumentNullException(nameof(Variables));
		}

		public SmartSqlMapper(String sqlMapConfigFilePath = "SmartSqlMapConfig.xml") : this(NullLoggerFactory.Instance, sqlMapConfigFilePath)
		{

		}
		public SmartSqlMapper(
			 ILoggerFactory loggerFactory,
			 String sqlMapConfigFilePath = "SmartSqlMapConfig.xml"
		) : this(new SmartSqlOptions
		{
			LoggerFactory = loggerFactory,
			ConfigPath = sqlMapConfigFilePath
		})
		{

		}
		public SmartSqlMapper(SmartSqlOptions options)
		{
			_smartSqlOptions = options;
			_smartSqlOptions.Setup();
			_logger = LoggerFactory.CreateLogger<SmartSqlMapper>();
		}

		private void SetupRequestContext(RequestContext context)
		{
			context.Setup(_smartSqlOptions.SmartSqlContext, SqlBuilder);
		}

		#region Sync
		public T ExecuteWrap<T>(Func<IDbConnectionSession, T> execute, RequestContext context)
		{
			IDbConnectionSession dbSession = null;
			try
			{
				context.Variables = Variables;
				SetupRequestContext(context);
				if (CacheManager.TryGet(context, out T cachedResult))
				{
					return cachedResult;
				}
				var dataSource = DataSourceFilter.Elect(context);
				dbSession = SessionStore.GetOrAddDbSession(dataSource);
				var result = execute(dbSession);
				CacheManager.RequestExecuted(dbSession, context);
				CacheManager.TryAdd(context, result);
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.HelpLink, ex, $"Statement:{context.FullSqlId} Message:{ex.Message}");
				throw new SmartSqlException($"Statement:{context.FullSqlId} Message:{ex.Message}{context.RealSql}", ex);
			}
			finally
			{
				if (dbSession is object && dbSession.LifeCycle == DbSessionLifeCycle.Transient)
				{
					SessionStore.Dispose();
				}
			}
		}

		public int Execute(RequestContext context)
		{
			return ExecuteWrap((dbSession) =>
			 {
				 return CommandExecuter.ExecuteNonQuery(dbSession, context);
			 }, context);
		}
		public T ExecuteScalar<T>(RequestContext context)
		{
			return ExecuteWrap((dbSession) =>
			{
				var result = CommandExecuter.ExecuteScalar(dbSession, context);
				return (T)Convert.ChangeType(result, typeof(T));
			}, context);
		}

		// 反序列化反射调用缓存：按 (反序列化器运行时类型, 结果类型) 缓存编译后的强类型委托，
		// 避免每次查询都执行 GetMethod + MakeGenericMethod + MethodInfo.Invoke（含参数装箱数组分配）
		private static readonly ConcurrentDictionary<(Type DeserializerType, Type ResultType), Func<IDataReaderDeserializer, RequestContext, IDataReader, object>> _toEnumerableDelegateCache
			= new ConcurrentDictionary<(Type, Type), Func<IDataReaderDeserializer, RequestContext, IDataReader, object>>();
		private static readonly ConcurrentDictionary<(Type DeserializerType, Type ResultType), Func<IDataReaderDeserializer, RequestContext, IDataReader, object>> _toSingleDelegateCache
			= new ConcurrentDictionary<(Type, Type), Func<IDataReaderDeserializer, RequestContext, IDataReader, object>>();

		/// <summary>
		/// 构建调用反序列化器泛型方法（ToEnumerable/ToSingle）的编译委托
		/// </summary>
		/// <param name="methodName">方法名：ToEnumerable 或 ToSingle</param>
		/// <param name="materializeToList">是否在委托内调用 Enumerable.ToList 物化结果（仅 ToEnumerable 使用）</param>
		private static Func<IDataReaderDeserializer, RequestContext, IDataReader, object> BuildDeserializerDelegate(
			(Type DeserializerType, Type ResultType) key, string methodName, bool materializeToList)
		{
			var deserParam = Expression.Parameter(typeof(IDataReaderDeserializer), "deser");
			var contextParam = Expression.Parameter(typeof(RequestContext), "context");
			var readerParam = Expression.Parameter(typeof(IDataReader), "dataReader");
			var method = key.DeserializerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
			Expression body = Expression.Call(
				Expression.Convert(deserParam, key.DeserializerType),
				method.MakeGenericMethod(key.ResultType),
				contextParam, readerParam);
			if (materializeToList)
			{
				// 在委托内物化为 List<resultType>，结果类型匹配时调用方可直接使用，免去 Cast<T>().ToList() 二次物化
				body = Expression.Call(typeof(Enumerable), nameof(Enumerable.ToList), new[] { key.ResultType }, body);
			}
			body = Expression.Convert(body, typeof(object));
			return Expression.Lambda<Func<IDataReaderDeserializer, RequestContext, IDataReader, object>>(
				body, deserParam, contextParam, readerParam).Compile();
		}

		public IEnumerable<T> Query<T>(RequestContext context)
		{
			return ExecuteWrap((dbSession) =>
			{
				using (var dataReader = CommandExecuter.ExecuteReader(dbSession, context))
				{
					var deser = DeserializerFactory.Create();
					Type resultType = context.Statement.ResultType ?? context.Statement?.ResultMap?.ResultType;
					IList<T> objList = null;
					if (resultType is not null)
					{
						var func = _toEnumerableDelegateCache.GetOrAdd((deser.GetType(), resultType),
							key => BuildDeserializerDelegate(key, "ToEnumerable", true));
						var list = func(deser, context, dataReader);
						// 委托内已物化为 List<resultType>，类型匹配时直接复用，否则退回 Cast<T>().ToList()
						objList = list as IList<T> ?? (list as System.Collections.IEnumerable).Cast<T>().ToList();
					}
					else
					{
						objList = deser.ToEnumerable<T>(context, dataReader).ToList();
					}
					dataReader.Close();
					return objList;
				}
			}, context);
		}
		public T QuerySingle<T>(RequestContext context)
		{
			return ExecuteWrap((dbSession) =>
			{
				using (var dataReader = CommandExecuter.ExecuteReader(dbSession, context))
				{
					var deser = DeserializerFactory.Create();
					Type resultType = context.Statement.ResultType ?? context.Statement?.ResultMap?.ResultType;
					T obj = default;
					if (resultType is not null)
					{
						var func = _toSingleDelegateCache.GetOrAdd((deser.GetType(), resultType),
							key => BuildDeserializerDelegate(key, "ToSingle", false));
						obj = (T)func(deser, context, dataReader);
					}
					else
					{
						obj = deser.ToSingle<T>(context, dataReader);
					}
					dataReader.Close();
					return obj;
				}
			}, context);
		}

		public DataTable GetDataTable(RequestContext context)
		{
			return ExecuteWrap((dbSession) =>
			{
				using (var dataReader = CommandExecuter.ExecuteReader(dbSession, context))
				{
					var dt = DataReaderConvert.ToDataTable(dataReader);
					dataReader.Close();
					return dt;
				}
			}, context);
		}

		public DataSet GetDataSet(RequestContext context)
		{
			return ExecuteWrap((dbSession) =>
			{
				using var dataReader = CommandExecuter.ExecuteReader(dbSession, context);
				return DataReaderConvert.ToDataSet(dataReader);
			}, context);
		}
		#endregion
		#region Async
		public async Task<T> ExecuteWrapAsync<T>(Func<IDbConnectionSession, Task<T>> execute, RequestContext context)
		{
			SetupRequestContext(context);
			if (CacheManager.TryGet<T>(context, out T cachedResult))
			{
				return cachedResult;
			}
			var dataSource = DataSourceFilter.Elect(context);
			var dbSession = SessionStore.GetOrAddDbSession(dataSource);
			try
			{
				var result = await execute(dbSession).ConfigureAwait(false);
				CacheManager.RequestExecuted(dbSession, context);
				CacheManager.TryAdd<T>(context, result);
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.HelpLink, ex, ex.Message);
				throw;
			}
			finally
			{
				if (dbSession.LifeCycle == DbSessionLifeCycle.Transient)
				{
					SessionStore.Dispose();
				}
			}
		}
		public async Task<int> ExecuteAsync(RequestContext context)
		{
			return await ExecuteWrapAsync(async (dbSession) =>
		   {
			   return await CommandExecuter.ExecuteNonQueryAsync(dbSession, context).ConfigureAwait(false);
		   }, context).ConfigureAwait(false);
		}
		public async Task<T> ExecuteScalarAsync<T>(RequestContext context)
		{
			return await ExecuteWrapAsync(async (dbSession) =>
			{
				var result = await CommandExecuter.ExecuteScalarAsync(dbSession, context).ConfigureAwait(false);
				return (T)Convert.ChangeType(result, typeof(T));
			}, context).ConfigureAwait(false);
		}
		public async Task<IEnumerable<T>> QueryAsync<T>(RequestContext context)
		{
			return await ExecuteWrapAsync(async (dbSession) =>
			{
				var dataReader = await CommandExecuter.ExecuteReaderAsync(dbSession, context).ConfigureAwait(false);
				var deser = DeserializerFactory.Create();
				return await deser.ToEnumerableAsync<T>(context, dataReader).ConfigureAwait(false);
			}, context).ConfigureAwait(false);
		}
		public async Task<T> QuerySingleAsync<T>(RequestContext context)
		{
			return await ExecuteWrapAsync(async (dbSession) =>
			{
				var dataReader = await CommandExecuter.ExecuteReaderAsync(dbSession, context).ConfigureAwait(false);
				var deser = DeserializerFactory.Create();
				return await deser.ToSingleAsync<T>(context, dataReader).ConfigureAwait(false);
			}, context).ConfigureAwait(false);
		}

		public async Task<DataTable> GetDataTableAsync(RequestContext context)
		{
			return await ExecuteWrapAsync(async (dbSession) =>
			{
				DbDataReader dataReader = null;
				try
				{
					dataReader = await CommandExecuter.ExecuteReaderAsync(dbSession, context).ConfigureAwait(false);
					return await DataReaderConvert.ToDataTableAsync(dataReader).ConfigureAwait(false);
				}
				finally
				{
					if (dataReader is not null)
					{
						dataReader.Dispose();
					}
				}

			}, context).ConfigureAwait(false);
		}

		public async Task<DataSet> GetDataSetAsync(RequestContext context)
		{
			return await ExecuteWrapAsync(async (dbSession) =>
			{
				DbDataReader dataReader = null;
				try
				{
					dataReader = await CommandExecuter.ExecuteReaderAsync(dbSession, context).ConfigureAwait(false);
					return await DataReaderConvert.ToDataSetAsync(dataReader).ConfigureAwait(false);
				}
				finally
				{
					if (dataReader is not null)
					{
						dataReader.Dispose();
					}
				}
			}, context).ConfigureAwait(false);
		}
		#endregion
		#region Transaction
		public IDbConnectionSession BeginTransaction()
		{
			return BeginTransaction(IsolationLevel.RepeatableRead);
		}

		public IDbConnectionSession BeginTransaction(IsolationLevel isolationLevel)
		{
			var reqContext = new RequestContext
			{
				DataSourceChoice = DataSourceChoice.Write
			};

			return BeginTransaction(reqContext, isolationLevel);
		}

		public IDbConnectionSession BeginTransaction(RequestContext context)
		{
			return BeginTransaction(context, IsolationLevel.RepeatableRead);
		}

		public IDbConnectionSession BeginTransaction(RequestContext context, IsolationLevel isolationLevel)
		{
			var dbSession = BeginSession(context);
			dbSession.BeginTransaction(isolationLevel);
			if (_logger.IsEnabled(LogLevel.Debug))
			{
				_logger.LogDebug($"BeginTransaction DbSession.Id:{dbSession.Id}");
			}
			return dbSession;
		}

		public void CommitTransaction()
		{
			var session = SessionStore.LocalSession;
			if (session is null)
			{
				throw new SmartSqlException("SmartSqlMapper could not invoke CommitTransaction(). No Transaction was started. Call BeginTransaction() first.");
			}
			try
			{
				if (_logger.IsEnabled(LogLevel.Debug))
				{
					_logger.LogDebug($"【数据提交CommitTransaction DbSession.Id:{session.Id}:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}】");
				}
				session.CommitTransaction();
				CacheManager.RequestCommitted(session);
			}
			finally
			{
				SessionStore.Dispose();
			}
		}

		public void RollbackTransaction()
		{
			var session = SessionStore.LocalSession;
			if (session is null)
			{
				throw new SmartSqlException("SmartSqlMapper could not invoke RollBackTransaction(). No Transaction was started. Call BeginTransaction() first.");
			}
			try
			{
				if (_logger.IsEnabled(LogLevel.Debug))
				{
					_logger.LogDebug($"RollbackTransaction DbSession.Id:{session.Id}");
				}
				session.RollbackTransaction();
			}
			finally
			{
				SessionStore.Dispose();
			}
		}

		#endregion
		#region Scoped Session
		public IDbConnectionSession BeginSession(RequestContext context)
		{
			if (SessionStore.LocalSession is object)
			{
				throw new SmartSqlException("SmartSqlMapper could not invoke BeginSession(). A LocalSession is already existed.");
			}
			var dataSource = DataSourceFilter.Elect(context);
			var dbSession = SessionStore.CreateDbSession(dataSource);
			dbSession.Begin();
			return dbSession;
		}

		public void EndSession()
		{
			var dbSession = SessionStore.LocalSession;
			if (dbSession is null)
			{
				throw new SmartSqlException("SmartSqlMapper could not invoke EndSession(). No LocalSession was existed. ");
			}
			dbSession.End();
			SessionStore.Dispose();
		}
		#endregion

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			ConfigLoader.Dispose();
			SessionStore.Dispose();
			CacheManager.Dispose();
			//if (_logger.IsEnabled(LogLevel.Warning))
			//{
			//    _logger.LogWarning($"SmartSqlMapper Dispose.");
			//}
		}




	}
}
