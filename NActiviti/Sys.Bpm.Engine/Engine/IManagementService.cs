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
namespace org.activiti.engine
{
    using org.activiti.engine.@event;
    using org.activiti.engine.impl.cmd;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.management;
    using org.activiti.engine.runtime;
    using System.Data;

    /// <summary>
    /// Service for admin and maintenance operations on the process engine.
    /// 
    /// These operations will typically not be used in a workflow driven application, but are used in for example the operational console.
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    public interface IManagementService
    {

        /// <summary>
        /// Get the mapping containing {table name, row count} entries of the Activiti database schema.
        /// </summary>
        IDictionary<string, long> TableCount { get; }

        /// <summary>
        /// Gets the table name (including any configured prefix) for an Activiti entity like Task, Execution or the like.
        /// </summary>
        string GetTableName(Type activitiEntityClass);

        /// <summary>
        /// Gets the metadata (column names, column types, etc.) of a certain table. Returns null when no table exists with the given name.
        /// </summary>
        TableMetaData GetTableMetaData(string tableName);

        /// <summary>
        /// Creates a <seealso cref="ITablePageQuery"/> that can be used to fetch <seealso cref="TablePage"/> containing specific sections of table row data.
        /// </summary>
        ITablePageQuery CreateTablePageQuery();

        /// <summary>
        /// Returns a new JobQuery implementation, that can be used to dynamically query the jobs.
        /// </summary>
        IJobQuery CreateJobQuery();

        /// <summary>
        /// Returns a new TimerJobQuery implementation, that can be used to dynamically query the timer jobs.
        /// </summary>
        ITimerJobQuery CreateTimerJobQuery();

        /// <summary>
        /// Returns a new SuspendedJobQuery implementation, that can be used to dynamically query the suspended jobs.
        /// </summary>
        ISuspendedJobQuery CreateSuspendedJobQuery();

        /// <summary>
        /// Returns a new DeadLetterJobQuery implementation, that can be used to dynamically query the dead letter jobs.
        /// </summary>
        IDeadLetterJobQuery CreateDeadLetterJobQuery();

        /// <summary>
        /// Forced synchronous execution of a job (eg. for administration or testing) The job will be executed, even if the process definition and/or the process instance is in suspended state.
        /// </summary>
        /// <param name="jobId">
        ///          id of the job to execute, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when there is no job with the given id. </exception>
        void ExecuteJob(string jobId);

        /// <summary>
        /// Moves a timer job to the executable job table (eg. for administration or testing). The timer job will be moved, even if the process definition and/or the process instance is in suspended state.
        /// </summary>
        /// <param name="jobId">
        ///          id of the timer job to move, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when there is no job with the given id. </exception>
        IJob MoveTimerToExecutableJob(string jobId);

        /// <summary>
        /// Moves a job to the dead letter job table (eg. for administration or testing). The job will be moved, even if the process definition and/or the process instance has retries left.
        /// </summary>
        /// <param name="jobId">
        ///          id of the job to move, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when there is no job with the given id. </exception>
        IJob MoveJobToDeadLetterJob(string jobId);

        /// <summary>
        /// Moves a job that is in the dead letter job table back to be an executable job, 
        /// and resetting the retries (as the retries was 0 when it was put into the dead letter job table).
        /// </summary>
        /// <param name="jobId">
        ///          id of the job to move, cannot be null. </param>
        /// <param name="retries">
        ///          the number of retries (value greater than 0) which will be set on the job. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when there is no job with the given id. </exception>
        IJob MoveDeadLetterJobToExecutableJob(string jobId, int retries);

        /// <summary>
        /// Delete the job with the provided id.
        /// </summary>
        /// <param name="jobId">
        ///          id of the job to delete, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when there is no job with the given id. </exception>
        void DeleteJob(string jobId);

        /// <summary>
        /// Delete the timer job with the provided id.
        /// </summary>
        /// <param name="jobId">
        ///          id of the timer job to delete, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when there is no job with the given id. </exception>
        void DeleteTimerJob(string jobId);

        /// <summary>
        /// Delete the dead letter job with the provided id.
        /// </summary>
        /// <param name="jobId">
        ///          id of the dead letter job to delete, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when there is no job with the given id. </exception>
        void DeleteDeadLetterJob(string jobId);

        /// <summary>
        /// Sets the number of retries that a job has left.
        /// 
        /// Whenever the JobExecutor fails to execute a job, this value is decremented. When it hits zero, the job is supposed to be dead and not retried again. In that case, this method can be used to
        /// increase the number of retries.
        /// </summary>
        /// <param name="jobId">
        ///          id of the job to modify, cannot be null. </param>
        /// <param name="retries">
        ///          number of retries. </param>
        void SetJobRetries(string jobId, int retries);

        /// <summary>
        /// Sets the number of retries that a timer job has left.
        /// 
        /// Whenever the JobExecutor fails to execute a timer job, this value is decremented. When it hits zero, the job is supposed to be dead and not retried again. In that case, this method can be used to
        /// increase the number of retries.
        /// </summary>
        /// <param name="jobId">
        ///          id of the timer job to modify, cannot be null. </param>
        /// <param name="retries">
        ///          number of retries. </param>
        void SetTimerJobRetries(string jobId, int retries);

        /// <summary>
        /// Returns the full stacktrace of the exception that occurs when the job with the given id was last executed. 
        /// Returns null when the job has no exception stacktrace.
        /// </summary>
        /// <param name="jobId">
        ///          id of the job, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no job exists with the given id. </exception>
        string GetJobExceptionStacktrace(string jobId);

        /// <summary>
        /// Returns the full stacktrace of the exception that occurs when the <seealso cref="TimerJobEntity"/> with the given id was last executed. 
        /// Returns null when the job has no exception stacktrace.
        /// </summary>
        /// <param name="jobId">
        ///          id of the job, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no job exists with the given id. </exception>
        string GetTimerJobExceptionStacktrace(string jobId);

        /// <summary>
        /// Returns the full stacktrace of the exception that occurs when the <seealso cref="SuspendedJobEntity"/> with the given id was last executed. 
        /// Returns null when the job has no exception stacktrace.
        /// </summary>
        /// <param name="jobId">
        ///          id of the job, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no job exists with the given id. </exception>
        string GetSuspendedJobExceptionStacktrace(string jobId);

        /// <summary>
        /// Returns the full stacktrace of the exception that occurs when the <seealso cref="DeadLetterJobEntity"/> with the given id was last executed. 
        /// Returns null when the job has no exception stacktrace.
        /// </summary>
        /// <param name="jobId">
        ///          id of the job, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no job exists with the given id. </exception>
        string GetDeadLetterJobExceptionStacktrace(string jobId);


        /// <summary>
        /// get the list of properties. </summary>
        IDictionary<string, string> Properties { get; }

        /// <summary>
        /// programmatic schema update on a given connection returning feedback about what happened
        /// </summary>
        string DatabaseSchemaUpgrade(IDbConnection connection, string catalog, string schema);

        /// <summary>
        /// Executes a given command with the default <seealso cref="CommandConfig"/>.
        /// </summary>
        /// <param name="command">
        ///          the command, cannot be null. </param>
        /// <returns> the result of command execution </returns>
        T ExecuteCommand<T>(ICommand<T> command);

        /// <summary>
        /// Executes a given command with the specified <seealso cref="CommandConfig"/>.
        /// </summary>
        /// <param name="config">
        ///          the command execution configuration, cannot be null. </param>
        /// <param name="command">
        ///          the command, cannot be null. </param>
        /// <returns> the result of command execution </returns>
        T ExecuteCommand<T>(CommandConfig config, ICommand<T> command);

        /// <summary>
        /// Executes the sql contained in the <seealso cref="CustomSqlExecution"/> parameter.
        /// </summary>
        ResultType ExecuteCustomSql<MapperType, ResultType>(ICustomSqlExecution<MapperType, ResultType> customSqlExecution);

        /// <summary>
        /// Returns a list of event log entries, describing everything the engine has processed. Note that the event logging must specifically must be enabled in the process engine configuration.
        /// 
        /// Passing null as arguments will effectively fetch ALL event log entries. Be careful, as this list might be huge!
        /// </summary>
        IList<IEventLogEntry> GetEventLogEntries(long? startLogNr, long? pageSize);

        /// <summary>
        /// Returns a list of event log entries for a specific process instance id. Note that the event logging must specifically must be enabled in the process engine configuration.
        /// 
        /// Passing null as arguments will effectively fetch ALL event log entries. Be careful, as this list might be huge!
        /// </summary>
        IList<IEventLogEntry> GetEventLogEntriesByProcessInstanceId(string processInstanceId);

        /// <summary>
        /// Delete a EventLogEntry. Typically only used in testing, as deleting log entries defeats the whole purpose of keeping a log.
        /// </summary>
        void DeleteEventLogEntry(long logNr);
    }
}