using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using java;
using java.sql;
using javax.transaction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Sys.Workflow.engine.impl;
using Sys.Workflow.engine.impl.persistence.entity;
using Sys.Workflow.engine.impl.util.io;
using Sys;

namespace javax.sql
{
    public class JdbcType
    {
        public JdbcType()
        {
        }
    }

    public abstract class DataSource
    {

        public abstract IConnection Connection { get; set; }

        public virtual void forceCloseAll()
        {

        }

        public abstract IConnection getConnection(string username, string password);
    }

    public class DriverManager
    {
        private static  ILogger<DriverManager> log = ProcessEngineServiceProvider.LoggerService<DriverManager>();

        public static IConnection getConnection(string jdbcUrl, string jdbcUsername, string jdbcPassword)
        {
            log.LogWarning("mock java class DriverManager.getConnection");

            return new Connection();
        }
    }

    public interface ISqlSessionFactory
    {
        SqlSession openSession();

        SqlSession openSession(IConnection connection);
    }

    public class Persistence
    {
        private static  ILogger<DriverManager> log = ProcessEngineServiceProvider.LoggerService<DriverManager>();

        public static EntityManagerFactory createEntityManagerFactory(string jpaPersistenceUnitName)
        {
            log.LogWarning("mock java class Persistence.createEntityManagerFactory");

            return new EntityManagerFactory();
        }
    }

    public class EntityManagerFactory
    { }

    public class DefaultSqlSessionFactory : ISqlSessionFactory
    {
        private static  ILogger<DriverManager> log = ProcessEngineServiceProvider.LoggerService<DriverManager>();

        private Configuration configuration;

        public DefaultSqlSessionFactory(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public SqlSession openSession()
        {
            log.LogWarning("mock java class DefaultSqlSessionFactory.openSession");

            return new SqlSession();
        }

        public SqlSession openSession(IConnection connection)
        {
            log.LogWarning("mock java class DefaultSqlSessionFactory.openSession IConnection connection");

            return new SqlSession();
        }
    }

    public class RowBounds
    {
        private int firstResult;
        private int maxResults;

        public RowBounds(int firstResult, int maxResults)
        {
            this.firstResult = firstResult;
            this.maxResults = maxResults;
        }
    }

    public class InitialContext
    {
        private static  ILogger<InitialContext> log = ProcessEngineServiceProvider.LoggerService<InitialContext>();

        public DataSource lookup(string dataSourceJndiName)
        {
            log.LogWarning("mock java class InitialContext.lookup");

            return new PooledDataSource();
        }
    }

    public class ResultSet
    {
        private static  ILogger<InitialContext> log = ProcessEngineServiceProvider.LoggerService<InitialContext>();

        public DatabaseMetaData MetaData { get; set; }

        public bool next()
        {
            log.LogWarning("mock java class ResultSet.next");

            return false;
        }

        public string getString(int v)
        {
            log.LogWarning("mock java class ResultSet.getString");

            return null;
        }

        public string getInt(int v)
        {
            log.LogWarning("mock java class ResultSet.getInt");

            return null;
        }

        public void close()
        {
            log.LogWarning("mock java class ResultSet.getInt");
        }

        public string getString(string v)
        {
            log.LogWarning("mock java class ResultSet.getString");

            return null;
        }
    }

    public class DatabaseMetaData
    {
        private static  ILogger<DatabaseMetaData> log = ProcessEngineServiceProvider.LoggerService<DatabaseMetaData>();

        public string DatabaseProductName { get; set; }
        public int DatabaseMajorVersion { get; set; }
        public int DatabaseMinorVersion { get; set; }
        public string SearchStringEscape { get; set; }
        public int ColumnCount { get; set; }

        public ResultSet getTables(object p1, object p2, object p3, object p4)
        {
            log.LogWarning("mock java class DatabaseMetaData.getTables");

            return new ResultSet();
        }

        public ResultSet getColumns(object p1, object p2, string tableName, object p3)
        {
            log.LogWarning("mock java class DatabaseMetaData.getColumns");

            return new ResultSet();
        }

        public ResultSet getIndexInfo(object p1, object p2, string tableName, bool v1, bool v2)
        {
            log.LogWarning("mock java class DatabaseMetaData.getIndexInfo");

            return new ResultSet();
        }

        public string getColumnName(int v)
        {
            log.LogWarning("mock java class DatabaseMetaData.getColumnName");

            return null;
        }
    }

    public class PooledDataSource : DataSource
    {
        private static  ILogger<PooledDataSource> log = ProcessEngineServiceProvider.LoggerService<PooledDataSource>();

        private ClassLoader classLoader;
        private string jdbcDriver;
        private string jdbcUrl;
        private string jdbcUsername;
        private string jdbcPassword;

        public PooledDataSource()
        {
            Connection = new Connection();
        }

        public PooledDataSource(ClassLoader classLoader, string jdbcDriver, string jdbcUrl, string jdbcUsername, string jdbcPassword) : this()
        {
            this.classLoader = classLoader;
            this.jdbcDriver = jdbcDriver;
            this.jdbcUrl = jdbcUrl;
            this.jdbcUsername = jdbcUsername;
            this.jdbcPassword = jdbcPassword;
        }

        public int PoolMaximumActiveConnections { get; set; }
        public int PoolMaximumIdleConnections { get; set; }
        public int PoolMaximumCheckoutTime { get; set; }
        public int PoolTimeToWait { get; set; }
        public bool PoolPingEnabled { get; set; }
        public string PoolPingQuery { get; set; }
        public int PoolPingConnectionsNotUsedFor { get; set; }
        public int DefaultTransactionIsolationLevel { get; set; }
        public override IConnection Connection { get; set; }

        public override void forceCloseAll()
        {
            log.LogWarning("mock java class PooledDataSource.forceCloseAll");
        }

        public override IConnection getConnection(string username, string password)
        {
            log.LogWarning("mock java class PooledDataSource.getConnection");

            return new Connection();
        }
    }

    public class Connection : IConnection
    {
         ILogger<Connection> log = ProcessEngineServiceProvider.LoggerService<Connection>();

        public DatabaseMetaData MetaData
        {
            get => new DatabaseMetaData();
            set
            {
                log.LogWarning("mock java Connection.MetaData!");
            }
        }

        public string Schema
        {
            get
            {
                return "";

            }
            set
            {
                log.LogWarning("mock java Connection.Schema!");
            }
        }
        public string Catalog
        {
            get { return ""; }
            set
            {
                log.LogWarning("mock java Connection.Catalog!");
            }
        }

        public void close()
        {
            log.LogWarning("mock java Connection.close!");
        }

        public Statement createStatement()
        {
            log.LogWarning("mock java Connection.createStatement!");
            return new Statement();
        }
    }

    public class Resource
    {
        private static  ILogger<Resource> log = ProcessEngineServiceProvider.LoggerService<Resource>();

        public static implicit operator Resource(InputStreamSource v)
        {
            log.LogWarning("mock java public static implicit operator Resource(InputStreamSource v)!");

            return v;
        }
    }

    public class SqlSession
    {
        private static  ILogger<SqlSession> log = ProcessEngineServiceProvider.LoggerService<SqlSession>();

        public IConnection Connection { get; set; }

        public int delete(string statement, object parameter)
        {
            log.LogWarning("mock java SqlSession.delete!");

            return -1;
        }

        public int update(string updateStatement, object parameters)
        {
            log.LogWarning("mock java SqlSession.update!");

            return -1;
        }

        public IList<object> selectList(string statement, object parameter)
        {
            log.LogWarning("mock java SqlSession.selectList!");

            return null;
        }

        public object selectOne(string statement, object parameter)
        {
            log.LogWarning("mock java SqlSession.selectOne!");

            return null;
        }

        public void insert(string insertStatement, IEntity entity)
        {
            log.LogWarning("mock java SqlSession.insert!");
        }

        public void insert(string insertStatement, IList<IEntity> subList)
        {
            log.LogWarning("mock java SqlSession.insert!");
        }

        public void close()
        {
            log.LogWarning("mock java SqlSession.close!");
        }

        public void commit()
        {
            log.LogWarning("mock java SqlSession.commit!");
        }

        public void rollback()
        {
            log.LogWarning("mock java SqlSession.rollback!");
        }

        public string selectOne(string selectSchemaVersionStatement)
        {
            log.LogWarning("mock java SqlSession.selectOne!");

            return null;
        }

        public object getMapper(Type type)
        {
            log.LogWarning("mock java SqlSession.getMapper!");

            return null;
        }

        public IList selectList(string v, TablePageQueryImpl tablePageQuery, RowBounds rowBounds)
        {
            log.LogWarning("mock java SqlSession.selectList!");

            return null;
        }
    }

    public class Statement
    {
        private static  ILogger<Resource> log = ProcessEngineServiceProvider.LoggerService<Resource>();

        public void execute(string sqlStatement)
        {
            log.LogWarning("mock java Statement.execute!");
        }

        public void close()
        {
            log.LogWarning("mock java Statement.close!");
        }
    }

    public class PreparedStatement : Statement
    {
        private static  ILogger<Resource> log = ProcessEngineServiceProvider.LoggerService<Resource>();

        public void setString(int i, string v)
        {
            log.LogWarning("mock java PreparedStatement.setString!");
        }
    }

    public class CallableStatement : Statement
    {
        private static  ILogger<Resource> log = ProcessEngineServiceProvider.LoggerService<Resource>();

        public string getString(int columnIndex)
        {
            log.LogWarning("mock java CallableStatement.columnIndex!");

            return null;
        }
    }

    public interface IEntityManager
    {
        Transaction Transaction { get; set; }
        bool Open { get; set; }

        void close();
        object find(Type entityClass, object primaryKey);
        void flush();
    }

    public interface IEntityManagerFactory
    {
        IEntityManager createEntityManager();
    }
}