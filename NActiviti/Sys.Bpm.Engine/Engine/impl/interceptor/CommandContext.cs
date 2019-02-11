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
namespace org.activiti.engine.impl.interceptor
{
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.asyncexecutor;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.db;
    using org.activiti.engine.impl.history;
    using org.activiti.engine.impl.jobexecutor;
    using org.activiti.engine.impl.persistence.cache;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.logging;
    using Sys;
    using System.Collections.Concurrent;

    public class CommandContext<T1> : ICommandContext
    {
        private static readonly ILogger<CommandContext<T1>> log = ProcessEngineServiceProvider.LoggerService<CommandContext<T1>>();

        protected internal ICommand<T1> command;

        protected internal IDictionary<Type, ISessionFactory> sessionFactories;

        protected internal IDictionary<Type, ISession> sessions = new Dictionary<Type, ISession>();

        protected internal Exception exception_Renamed;
        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;
        protected internal IFailedJobCommandFactory failedJobCommandFactory;
        protected internal IList<ICommandContextCloseListener> closeListeners;
        protected internal IDictionary<string, object> attributes; // General-purpose storing of anything during the lifetime of a command context
        protected internal bool reused;

        protected internal IActivitiEngineAgenda agenda;
        protected internal ConcurrentDictionary<string, IExecutionEntity> involvedExecutions = new ConcurrentDictionary<string, IExecutionEntity>(StringComparer.OrdinalIgnoreCase); // The executions involved with the command
        protected internal LinkedList<object> resultStack = new LinkedList<object>(); // needs to be a stack, as JavaDelegates can do api calls again

        public CommandContext(ICommand<T1> command, ProcessEngineConfigurationImpl processEngineConfiguration)
        {
            this.command = command;
            this.processEngineConfiguration = processEngineConfiguration;
            this.failedJobCommandFactory = processEngineConfiguration.FailedJobCommandFactory;
            this.sessionFactories = processEngineConfiguration.SessionFactories ?? new Dictionary<Type, ISessionFactory>();
            this.agenda = processEngineConfiguration.EngineAgendaFactory.createAgenda(this);
        }

        public virtual void close()
        {

            // The intention of this method is that all resources are closed properly, even if exceptions occur
            // in close or flush methods of the sessions or the transaction context.

            try
            {
                try
                {
                    try
                    {
                        executeCloseListenersClosing();
                        if (exception_Renamed == null)
                        {
                            flushSessions();
                        }
                    }
                    catch (Exception exception)
                    {
                        this.exception(exception);
                    }
                    finally
                    {

                        try
                        {
                            if (exception_Renamed == null)
                            {
                                executeCloseListenersAfterSessionFlushed();
                            }
                        }
                        catch (Exception exception)
                        {
                            this.exception(exception);
                        }

                        if (exception_Renamed != null)
                        {
                            logException();
                            executeCloseListenersCloseFailure();
                        }
                        else
                        {
                            executeCloseListenersClosed();
                        }
                    }
                }
                catch (Exception exception)
                {
                    // Catch exceptions during rollback
                    this.exception(exception);
                }
                finally
                {
                    // Sessions need to be closed, regardless of exceptions/commit/rollback
                    closeSessions();
                }
            }
            catch (Exception exception)
            {
                // Catch exceptions during session closing
                this.exception(exception);
            }

            if (exception_Renamed != null)
            {
                rethrowExceptionIfNeeded();
            }
        }

        protected internal virtual void logException()
        {
            if (exception_Renamed is JobNotFoundException || exception_Renamed is ActivitiTaskAlreadyClaimedException)
            {
                // reduce log level, because this may have been caused because of job deletion due to cancelActiviti="true"
                log.LogInformation("Error while closing command context", exception_Renamed);
            }
            else if (exception_Renamed is ActivitiOptimisticLockingException)
            {
                // reduce log level, as normally we're not interested in logging this exception
                log.LogError(exception_Renamed, $"Optimistic locking exception : {exception_Renamed}");
            }
            else
            {
                log.LogError(exception_Renamed, $"Error while closing command context: {exception_Renamed}");
            }
        }

        protected internal virtual void rethrowExceptionIfNeeded()
        {
            if (exception_Renamed is Exception)
            {
                throw (Exception)exception_Renamed;
            }
            else if (exception_Renamed is Exception)
            {
                throw (Exception)exception_Renamed;
            }
            else
            {
                throw new ActivitiException("exception while executing command " + command, exception_Renamed);
            }
        }

        public virtual void addCloseListener(ICommandContextCloseListener commandContextCloseListener)
        {
            if (closeListeners == null)
            {
                closeListeners = new List<ICommandContextCloseListener>(1);
            }
            closeListeners.Add(commandContextCloseListener);
        }

        public virtual IList<ICommandContextCloseListener> CloseListeners
        {
            get
            {
                return closeListeners;
            }
        }

        public virtual bool hasCloseListener(Type type)
        {
            if (closeListeners != null && closeListeners.Count != 0)
            {
                foreach (ICommandContextCloseListener listener in closeListeners)
                {
                    if (type.IsInstanceOfType(listener))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected internal virtual void executeCloseListenersClosing()
        {
            if (closeListeners != null)
            {
                try
                {
                    foreach (ICommandContextCloseListener listener in closeListeners)
                    {
                        listener.closing(this);
                    }
                }
                catch (Exception exception)
                {
                    this.exception(exception);
                }
            }
        }

        protected internal virtual void executeCloseListenersAfterSessionFlushed()
        {
            if (closeListeners != null)
            {
                try
                {
                    foreach (ICommandContextCloseListener listener in closeListeners)
                    {
                        listener.afterSessionsFlush(this);
                    }
                }
                catch (Exception exception)
                {
                    this.exception(exception);
                }
            }
        }

        protected internal virtual void executeCloseListenersClosed()
        {
            if (closeListeners != null)
            {
                try
                {
                    foreach (ICommandContextCloseListener listener in closeListeners)
                    {
                        listener.closed(this);
                    }
                }
                catch (Exception exception)
                {
                    this.exception(exception);
                }
            }
        }

        protected internal virtual void executeCloseListenersCloseFailure()
        {
            if (closeListeners != null)
            {
                try
                {
                    foreach (ICommandContextCloseListener listener in closeListeners)
                    {
                        listener.closeFailure(this);
                    }
                }
                catch (Exception exception)
                {
                    this.exception(exception);
                }
            }
        }

        protected internal virtual void flushSessions()
        {
            foreach (ISession session in sessions.Values)
            {
                session.flush();
            }
        }

        protected internal virtual void closeSessions()
        {
            foreach (ISession session in sessions.Values)
            {
                try
                {
                    session.close();
                }
                catch (Exception exception)
                {
                    this.exception(exception);
                }
            }
        }

        /// <summary>
        /// Stores the provided exception on this <seealso cref="CommandContext"/> instance.
        /// That exception will be rethrown at the end of closing the <seealso cref="CommandContext"/> instance.
        /// <para>
        /// If there is already an exception being stored, a 'masked exception' message will be logged.
        /// </para>
        /// </summary>
        public virtual void exception(Exception exception)
        {
            if (this.exception_Renamed == null)
            {
                this.exception_Renamed = exception;
            }
            else
            {
                log.LogError("masked exception in command context. for root cause, see below as it will be rethrown later.", exception);
                LogMDC.clear();
            }
        }

        public virtual void addAttribute(string key, object value)
        {
            if (attributes == null)
            {
                attributes = new Dictionary<string, object>(1);
            }
            attributes[key] = value;
        }

        public virtual object getAttribute(string key)
        {
            if (attributes != null)
            {
                return attributes[key];
            }
            return null;
        }

        public virtual T getGenericAttribute<T>(string key)
        {
            if (attributes != null)
            {
                return (T)getAttribute(key);
            }
            return default(T);
        }

        public virtual T getSession<T>() where T : ISession
        {
            Type sessionClass = typeof(T);
            sessions.TryGetValue(sessionClass, out ISession session);
            if (session == null)
            {
                sessionFactories.TryGetValue(sessionClass, out ISessionFactory sessionFactory);
                if (sessionFactory == null)
                {
                    throw new ActivitiException("no session factory configured for " + sessionClass.FullName);
                }
                session = sessionFactory.openSession(this);
                sessions[sessionClass] = session;
            }

            return session == null ? default(T) : (T)session;
        }

        public virtual IDictionary<Type, ISessionFactory> SessionFactories
        {
            get
            {
                return sessionFactories;
            }
        }

        public virtual DbSqlSession DbSqlSession
        {
            get
            {
                return getSession<DbSqlSession>();
            }
        }

        public virtual IEntityCache EntityCache
        {
            get
            {
                return getSession<IEntityCache>();
            }
        }

        public virtual IDeploymentEntityManager DeploymentEntityManager
        {
            get
            {
                return processEngineConfiguration.DeploymentEntityManager;
            }
        }

        public virtual IResourceEntityManager ResourceEntityManager
        {
            get
            {
                return processEngineConfiguration.ResourceEntityManager;
            }
        }

        public virtual IByteArrayEntityManager ByteArrayEntityManager
        {
            get
            {
                return processEngineConfiguration.ByteArrayEntityManager;
            }
        }

        public virtual IProcessDefinitionEntityManager ProcessDefinitionEntityManager
        {
            get
            {
                return processEngineConfiguration.ProcessDefinitionEntityManager;
            }
        }

        public virtual IModelEntityManager ModelEntityManager
        {
            get
            {
                return processEngineConfiguration.ModelEntityManager;
            }
        }

        public virtual IProcessDefinitionInfoEntityManager ProcessDefinitionInfoEntityManager
        {
            get
            {
                return processEngineConfiguration.ProcessDefinitionInfoEntityManager;
            }
        }

        public virtual IExecutionEntityManager ExecutionEntityManager
        {
            get
            {
                return processEngineConfiguration.ExecutionEntityManager;
            }
        }

        public virtual ITaskEntityManager TaskEntityManager
        {
            get
            {
                return processEngineConfiguration.TaskEntityManager;
            }
        }

        public virtual IdentityLinkEntityManager IdentityLinkEntityManager
        {
            get
            {
                return processEngineConfiguration.IdentityLinkEntityManager;
            }
        }

        public virtual IVariableInstanceEntityManager VariableInstanceEntityManager
        {
            get
            {
                return processEngineConfiguration.VariableInstanceEntityManager;
            }
        }

        public virtual IHistoricProcessInstanceEntityManager HistoricProcessInstanceEntityManager
        {
            get
            {
                return processEngineConfiguration.HistoricProcessInstanceEntityManager;
            }
        }

        public virtual IHistoricDetailEntityManager HistoricDetailEntityManager
        {
            get
            {
                return processEngineConfiguration.HistoricDetailEntityManager;
            }
        }

        public virtual IHistoricVariableInstanceEntityManager HistoricVariableInstanceEntityManager
        {
            get
            {
                return processEngineConfiguration.HistoricVariableInstanceEntityManager;
            }
        }

        public virtual IHistoricActivityInstanceEntityManager HistoricActivityInstanceEntityManager
        {
            get
            {
                return processEngineConfiguration.HistoricActivityInstanceEntityManager;
            }
        }

        public virtual IHistoricTaskInstanceEntityManager HistoricTaskInstanceEntityManager
        {
            get
            {
                return processEngineConfiguration.HistoricTaskInstanceEntityManager;
            }
        }

        public virtual IHistoricIdentityLinkEntityManager HistoricIdentityLinkEntityManager
        {
            get
            {
                return processEngineConfiguration.HistoricIdentityLinkEntityManager;
            }
        }

        public virtual IEventLogEntryEntityManager EventLogEntryEntityManager
        {
            get
            {
                return processEngineConfiguration.EventLogEntryEntityManager;
            }
        }

        public virtual IJobEntityManager JobEntityManager
        {
            get
            {
                return processEngineConfiguration.JobEntityManager;
            }
        }

        public virtual ITimerJobEntityManager TimerJobEntityManager
        {
            get
            {
                return processEngineConfiguration.TimerJobEntityManager;
            }
        }

        public virtual ISuspendedJobEntityManager SuspendedJobEntityManager
        {
            get
            {
                return processEngineConfiguration.SuspendedJobEntityManager;
            }
        }

        public virtual IDeadLetterJobEntityManager DeadLetterJobEntityManager
        {
            get
            {
                return processEngineConfiguration.DeadLetterJobEntityManager;
            }
        }

        public virtual IAttachmentEntityManager AttachmentEntityManager
        {
            get
            {
                return processEngineConfiguration.AttachmentEntityManager;
            }
        }

        public virtual ITableDataManager TableDataManager
        {
            get
            {
                return processEngineConfiguration.TableDataManager;
            }
        }

        public virtual ICommentEntityManager CommentEntityManager
        {
            get
            {
                return processEngineConfiguration.CommentEntityManager;
            }
        }

        public virtual IPropertyEntityManager PropertyEntityManager
        {
            get
            {
                return processEngineConfiguration.PropertyEntityManager;
            }
        }

        public virtual IEventSubscriptionEntityManager EventSubscriptionEntityManager
        {
            get
            {
                return processEngineConfiguration.EventSubscriptionEntityManager;
            }
        }

        public virtual IHistoryManager HistoryManager
        {
            get
            {
                return processEngineConfiguration.HistoryManager;
            }
        }

        public virtual IJobManager JobManager
        {
            get
            {
                return processEngineConfiguration.JobManager;
            }
        }

        // Involved executions ////////////////////////////////////////////////////////

        public virtual void addInvolvedExecution(IExecutionEntity executionEntity)
        {
            if (!string.IsNullOrWhiteSpace(executionEntity.Id))
            {
                involvedExecutions.TryAdd(executionEntity.Id, executionEntity);
            }
        }

        public virtual bool hasInvolvedExecutions()
        {
            return involvedExecutions.Count > 0;
        }

        public virtual ICollection<IExecutionEntity> InvolvedExecutions
        {
            get
            {
                return involvedExecutions.Values;
            }
        }

        // getters and setters
        // //////////////////////////////////////////////////////
        public virtual ICommand<T1> Command
        {
            get
            {
                return command;
            }
        }

        public virtual IDictionary<Type, ISession> Sessions
        {
            get
            {
                return sessions;
            }
        }

        public virtual Exception Exception
        {
            get
            {
                return exception_Renamed;
            }
        }

        public virtual IFailedJobCommandFactory FailedJobCommandFactory
        {
            get
            {
                return failedJobCommandFactory;
            }
        }

        public virtual ProcessEngineConfigurationImpl ProcessEngineConfiguration
        {
            get
            {
                return processEngineConfiguration;
            }
        }

        public virtual IActivitiEventDispatcher EventDispatcher
        {
            get
            {
                return processEngineConfiguration.EventDispatcher;
            }
        }

        public virtual IActivitiEngineAgenda Agenda
        {
            get
            {
                return agenda;
            }
        }

        public virtual object GetResult()
        {
            if (resultStack.Count == 0)
            {
                return null;
            }

            var value = resultStack.Last.Value;

            resultStack.RemoveLast();

            return value;
        }

        public virtual void SetResult(object value)
        {
            resultStack.AddLast(value);
        }


        public virtual bool Reused
        {
            get
            {
                return reused;
            }
            set
            {
                this.reused = value;
            }
        }

    }

}