using System;
using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace org.activiti.engine.impl
{
    using org.activiti.engine.@event;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.cmd;
    using org.activiti.engine.impl.db;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.management;
    using org.activiti.engine.runtime;
    using System.Data;

    /// 
    /// 
    /// 
    /// 
    public class ManagementServiceImpl : ServiceImpl, IManagementService
    {
        public virtual IDictionary<string, long> TableCount
        {
            get
            {
                return commandExecutor.Execute(new GetTableCountCmd());
            }
        }

        public virtual string GetTableName(Type activitiEntityClass)
        {
            return commandExecutor.Execute(new GetTableNameCmd(activitiEntityClass));
        }

        public virtual TableMetaData GetTableMetaData(string tableName)
        {
            return commandExecutor.Execute(new GetTableMetaDataCmd(tableName));
        }

        public virtual void ExecuteJob(string jobId)
        {
            if (jobId is null)
            {
                throw new ActivitiIllegalArgumentException("JobId is null");
            }

            try
            {
                commandExecutor.Execute(new ExecuteJobCmd(jobId));

            }
            catch (Exception e)
            {
                if (e is ActivitiException)
                {
                    throw e;
                }
                else
                {
                    throw new ActivitiException("Job " + jobId + " failed", e);
                }
            }
        }

        public virtual IJob MoveTimerToExecutableJob(string jobId)
        {
            return commandExecutor.Execute(new MoveTimerToExecutableJobCmd(jobId));
        }

        public virtual IJob MoveJobToDeadLetterJob(string jobId)
        {
            return commandExecutor.Execute(new MoveJobToDeadLetterJobCmd(jobId));
        }

        public virtual IJob MoveDeadLetterJobToExecutableJob(string jobId, int retries)
        {
            return commandExecutor.Execute(new MoveDeadLetterJobToExecutableJobCmd(jobId, retries));
        }

        public virtual void DeleteJob(string jobId)
        {
            commandExecutor.Execute(new DeleteJobCmd(jobId));
        }

        public virtual void DeleteTimerJob(string jobId)
        {
            commandExecutor.Execute(new DeleteTimerJobCmd(jobId));
        }

        public virtual void DeleteDeadLetterJob(string jobId)
        {
            commandExecutor.Execute(new DeleteDeadLetterJobCmd(jobId));
        }

        public virtual void SetJobRetries(string jobId, int retries)
        {
            commandExecutor.Execute(new SetJobRetriesCmd(jobId, retries));
        }

        public virtual void SetTimerJobRetries(string jobId, int retries)
        {
            commandExecutor.Execute(new SetTimerJobRetriesCmd(jobId, retries));
        }

        public virtual ITablePageQuery CreateTablePageQuery()
        {
            return new TablePageQueryImpl(commandExecutor);
        }

        public virtual IJobQuery CreateJobQuery()
        {
            return new JobQueryImpl(commandExecutor);
        }

        public virtual ITimerJobQuery CreateTimerJobQuery()
        {
            return new TimerJobQueryImpl(commandExecutor);
        }

        public virtual ISuspendedJobQuery CreateSuspendedJobQuery()
        {
            return new SuspendedJobQueryImpl(commandExecutor);
        }

        public virtual IDeadLetterJobQuery CreateDeadLetterJobQuery()
        {
            return new DeadLetterJobQueryImpl(commandExecutor);
        }

        public virtual string GetJobExceptionStacktrace(string jobId)
        {
            return commandExecutor.Execute(new GetJobExceptionStacktraceCmd(jobId, JobType.ASYNC));
        }

        public virtual string GetTimerJobExceptionStacktrace(string jobId)
        {
            return commandExecutor.Execute(new GetJobExceptionStacktraceCmd(jobId, JobType.TIMER));
        }

        public virtual string GetSuspendedJobExceptionStacktrace(string jobId)
        {
            return commandExecutor.Execute(new GetJobExceptionStacktraceCmd(jobId, JobType.SUSPENDED));
        }

        public virtual string GetDeadLetterJobExceptionStacktrace(string jobId)
        {
            return commandExecutor.Execute(new GetJobExceptionStacktraceCmd(jobId, JobType.DEADLETTER));
        }

        public virtual IDictionary<string, string> Properties
        {
            get
            {
                return commandExecutor.Execute(new GetPropertiesCmd());
            }
        }

        public virtual string DatabaseSchemaUpgrade(IDbConnection connection, string catalog, string schema)
        {
            CommandConfig config = commandExecutor.DefaultConfig.TransactionNotSupported();
            return commandExecutor.Execute(config, new CommandAnonymousInnerClass(this, connection, catalog, schema));
        }

        private class CommandAnonymousInnerClass : ICommand<string>
        {
            private readonly ManagementServiceImpl outerInstance;

            private IDbConnection connection;
            private string catalog;
            private string schema;

            public CommandAnonymousInnerClass(ManagementServiceImpl outerInstance, IDbConnection connection, string catalog, string schema)
            {
                this.outerInstance = outerInstance;
                this.connection = connection;
                this.catalog = catalog;
                this.schema = schema;
            }

            public virtual string Execute(ICommandContext commandContext)
            {
                DbSqlSessionFactory dbSqlSessionFactory = (DbSqlSessionFactory)commandContext.SessionFactories[typeof(DbSqlSession)];
                DbSqlSession dbSqlSession = new DbSqlSession(dbSqlSessionFactory, commandContext.EntityCache, connection, catalog, schema);
                commandContext.Sessions[typeof(DbSqlSession)] = dbSqlSession;
                return dbSqlSession.DbSchemaUpdate();
            }
        }

        public virtual T ExecuteCommand<T>(ICommand<T> command)
        {
            if (command == null)
            {
                throw new ActivitiIllegalArgumentException("The command is null");
            }
            return commandExecutor.Execute(command);
        }

        public virtual T ExecuteCommand<T>(CommandConfig config, ICommand<T> command)
        {
            if (config == null)
            {
                throw new ActivitiIllegalArgumentException("The config is null");
            }
            if (command == null)
            {
                throw new ActivitiIllegalArgumentException("The command is null");
            }
            return commandExecutor.Execute(config, command);
        }

        public virtual ResultType ExecuteCustomSql<MapperType, ResultType>(ICustomSqlExecution<MapperType, ResultType> customSqlExecution)
        {
            Type mapperClass = customSqlExecution.MapperClass;
            return commandExecutor.Execute(new ExecuteCustomSqlCmd<MapperType, ResultType>(mapperClass, customSqlExecution));
        }

        public virtual IList<IEventLogEntry> GetEventLogEntries(long? startLogNr, long? pageSize)
        {
            return commandExecutor.Execute(new GetEventLogEntriesCmd(startLogNr, pageSize));
        }

        public virtual IList<IEventLogEntry> GetEventLogEntriesByProcessInstanceId(string processInstanceId)
        {
            return commandExecutor.Execute(new GetEventLogEntriesCmd(processInstanceId));
        }

        public virtual void DeleteEventLogEntry(long logNr)
        {
            commandExecutor.Execute(new DeleteEventLogEntry(logNr));
        }
    }

}