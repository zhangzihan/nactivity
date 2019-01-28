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
                return commandExecutor.execute(new GetTableCountCmd());
            }
        }

        public virtual string getTableName(Type activitiEntityClass)
        {
            return commandExecutor.execute(new GetTableNameCmd(activitiEntityClass));
        }

        public virtual TableMetaData getTableMetaData(string tableName)
        {
            return commandExecutor.execute(new GetTableMetaDataCmd(tableName));
        }

        public virtual void executeJob(string jobId)
        {
            if (ReferenceEquals(jobId, null))
            {
                throw new ActivitiIllegalArgumentException("JobId is null");
            }

            try
            {
                commandExecutor.execute(new ExecuteJobCmd(jobId));

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

        public virtual IJob moveTimerToExecutableJob(string jobId)
        {
            return commandExecutor.execute(new MoveTimerToExecutableJobCmd(jobId));
        }

        public virtual IJob moveJobToDeadLetterJob(string jobId)
        {
            return commandExecutor.execute(new MoveJobToDeadLetterJobCmd(jobId));
        }

        public virtual IJob moveDeadLetterJobToExecutableJob(string jobId, int retries)
        {
            return commandExecutor.execute(new MoveDeadLetterJobToExecutableJobCmd(jobId, retries));
        }

        public virtual void deleteJob(string jobId)
        {
            commandExecutor.execute(new DeleteJobCmd(jobId));
        }

        public virtual void deleteTimerJob(string jobId)
        {
            commandExecutor.execute(new DeleteTimerJobCmd(jobId));
        }

        public virtual void deleteDeadLetterJob(string jobId)
        {
            commandExecutor.execute(new DeleteDeadLetterJobCmd(jobId));
        }

        public virtual void setJobRetries(string jobId, int retries)
        {
            commandExecutor.execute(new SetJobRetriesCmd(jobId, retries));
        }

        public virtual void setTimerJobRetries(string jobId, int retries)
        {
            commandExecutor.execute(new SetTimerJobRetriesCmd(jobId, retries));
        }

        public virtual ITablePageQuery createTablePageQuery()
        {
            return new TablePageQueryImpl(commandExecutor);
        }

        public virtual IJobQuery createJobQuery()
        {
            return new JobQueryImpl(commandExecutor);
        }

        public virtual ITimerJobQuery createTimerJobQuery()
        {
            return new TimerJobQueryImpl(commandExecutor);
        }

        public virtual ISuspendedJobQuery createSuspendedJobQuery()
        {
            return new SuspendedJobQueryImpl(commandExecutor);
        }

        public virtual IDeadLetterJobQuery createDeadLetterJobQuery()
        {
            return new DeadLetterJobQueryImpl(commandExecutor);
        }

        public virtual string getJobExceptionStacktrace(string jobId)
        {
            return commandExecutor.execute(new GetJobExceptionStacktraceCmd(jobId, JobType.ASYNC));
        }

        public virtual string getTimerJobExceptionStacktrace(string jobId)
        {
            return commandExecutor.execute(new GetJobExceptionStacktraceCmd(jobId, JobType.TIMER));
        }

        public virtual string getSuspendedJobExceptionStacktrace(string jobId)
        {
            return commandExecutor.execute(new GetJobExceptionStacktraceCmd(jobId, JobType.SUSPENDED));
        }

        public virtual string getDeadLetterJobExceptionStacktrace(string jobId)
        {
            return commandExecutor.execute(new GetJobExceptionStacktraceCmd(jobId, JobType.DEADLETTER));
        }

        public virtual IDictionary<string, string> Properties
        {
            get
            {
                return commandExecutor.execute(new GetPropertiesCmd());
            }
        }

        public virtual string databaseSchemaUpgrade(IDbConnection connection, string catalog, string schema)
        {
            CommandConfig config = commandExecutor.DefaultConfig.transactionNotSupported();
            return commandExecutor.execute(config, new CommandAnonymousInnerClass(this, connection, catalog, schema));
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

            public virtual string execute(ICommandContext commandContext)
            {
                DbSqlSessionFactory dbSqlSessionFactory = (DbSqlSessionFactory)commandContext.SessionFactories[typeof(DbSqlSession)];
                DbSqlSession dbSqlSession = new DbSqlSession(dbSqlSessionFactory, commandContext.EntityCache, connection, catalog, schema);
                commandContext.Sessions[typeof(DbSqlSession)] = dbSqlSession;
                return dbSqlSession.dbSchemaUpdate();
            }
        }

        public virtual T executeCommand<T>(ICommand<T> command)
        {
            if (command == null)
            {
                throw new ActivitiIllegalArgumentException("The command is null");
            }
            return commandExecutor.execute(command);
        }

        public virtual T executeCommand<T>(CommandConfig config, ICommand<T> command)
        {
            if (config == null)
            {
                throw new ActivitiIllegalArgumentException("The config is null");
            }
            if (command == null)
            {
                throw new ActivitiIllegalArgumentException("The command is null");
            }
            return commandExecutor.execute(config, command);
        }

        public virtual ResultType executeCustomSql<MapperType, ResultType>(ICustomSqlExecution<MapperType, ResultType> customSqlExecution)
        {
            Type mapperClass = customSqlExecution.MapperClass;
            return commandExecutor.execute(new ExecuteCustomSqlCmd<MapperType, ResultType>(mapperClass, customSqlExecution));
        }

        public virtual IList<IEventLogEntry> getEventLogEntries(long? startLogNr, long? pageSize)
        {
            return commandExecutor.execute(new GetEventLogEntriesCmd(startLogNr, pageSize));
        }

        public virtual IList<IEventLogEntry> getEventLogEntriesByProcessInstanceId(string processInstanceId)
        {
            return commandExecutor.execute(new GetEventLogEntriesCmd(processInstanceId));
        }

        public virtual void deleteEventLogEntry(long logNr)
        {
            commandExecutor.execute(new DeleteEventLogEntry(logNr));
        }
    }

}