using System;
using System.Collections.Generic;
using org.activiti.engine.@delegate.@event;
using org.activiti.engine.impl.asyncexecutor;
using org.activiti.engine.impl.cfg;
using org.activiti.engine.impl.db;
using org.activiti.engine.impl.history;
using org.activiti.engine.impl.jobexecutor;
using org.activiti.engine.impl.persistence.cache;
using org.activiti.engine.impl.persistence.entity;

namespace org.activiti.engine.impl.interceptor
{
    public interface ICommandContext
    {
        IActivitiEngineAgenda Agenda { get; }
        IAttachmentEntityManager AttachmentEntityManager { get; }
        IByteArrayEntityManager ByteArrayEntityManager { get; }
        IList<ICommandContextCloseListener> CloseListeners { get; }
        ICommentEntityManager CommentEntityManager { get; }
        DbSqlSession DbSqlSession { get; }
        IDeadLetterJobEntityManager DeadLetterJobEntityManager { get; }
        IDeploymentEntityManager DeploymentEntityManager { get; }
        IEntityCache EntityCache { get; }
        IActivitiEventDispatcher EventDispatcher { get; }
        IEventLogEntryEntityManager EventLogEntryEntityManager { get; }
        IEventSubscriptionEntityManager EventSubscriptionEntityManager { get; }
        Exception Exception { get; }
        IExecutionEntityManager ExecutionEntityManager { get; }
        IFailedJobCommandFactory FailedJobCommandFactory { get; }
        IHistoricActivityInstanceEntityManager HistoricActivityInstanceEntityManager { get; }
        IHistoricDetailEntityManager HistoricDetailEntityManager { get; }
        IHistoricIdentityLinkEntityManager HistoricIdentityLinkEntityManager { get; }
        IHistoricProcessInstanceEntityManager HistoricProcessInstanceEntityManager { get; }

        T GetSession<T>() where T : ISession;

        IHistoricTaskInstanceEntityManager HistoricTaskInstanceEntityManager { get; }
        IHistoricVariableInstanceEntityManager HistoricVariableInstanceEntityManager { get; }
        IHistoryManager HistoryManager { get; }
        IIdentityLinkEntityManager IdentityLinkEntityManager { get; }
        ICollection<IExecutionEntity> InvolvedExecutions { get; }
        IJobEntityManager JobEntityManager { get; }
        IJobManager JobManager { get; }
        IModelEntityManager ModelEntityManager { get; }
        IProcessDefinitionEntityManager ProcessDefinitionEntityManager { get; }
        IProcessDefinitionInfoEntityManager ProcessDefinitionInfoEntityManager { get; }
        ProcessEngineConfigurationImpl ProcessEngineConfiguration { get; }
        IPropertyEntityManager PropertyEntityManager { get; }
        IResourceEntityManager ResourceEntityManager { get; }

        object GetResult();

        void SetResult(object value);


        bool Reused { get; set; }
        IDictionary<Type, ISessionFactory> SessionFactories { get; }
        IDictionary<Type, ISession> Sessions { get; }
        ISuspendedJobEntityManager SuspendedJobEntityManager { get; }
        ITableDataManager TableDataManager { get; }
        ITaskEntityManager TaskEntityManager { get; }
        ITimerJobEntityManager TimerJobEntityManager { get; }
        IVariableInstanceEntityManager VariableInstanceEntityManager { get; }

        void AddAttribute(string key, object value);
        void AddCloseListener(ICommandContextCloseListener commandContextCloseListener);
        void AddInvolvedExecution(IExecutionEntity executionEntity);
        void Close();
        void SetException(Exception exception);
        object GetAttribute(string key);
        T GetGenericAttribute<T>(string key);
        bool HasCloseListener(Type type);
        bool HasInvolvedExecutions();
    }
}