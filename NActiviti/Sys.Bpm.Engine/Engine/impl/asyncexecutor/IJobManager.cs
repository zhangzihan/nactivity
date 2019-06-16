namespace Sys.Workflow.Engine.Impl.Asyncexecutor
{
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;

    /// <summary>
    /// Contains methods that are not tied to any specific job type (async, timer, suspended or deadletter),
    /// but which are generally applicable or are about going from one type to another.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IJobManager
    {
        /// <summary>
        /// Execute a job, which means that the logic (async logic, timer that fires, etc)
        /// is executed, typically by a background thread of an executor.
        /// </summary>
        void Execute(IJob job);

        /// <summary>
        /// Unacquires a job, meaning that this job was previously locked, and 
        /// it is now freed to be acquired by other executor nodes. 
        /// </summary>
        void Unacquire(IJob job);

        /// <summary>
        /// Creates an async job for the provided <seealso cref="IExecutionEntity"/>, so that
        /// it can be continued later in a background thread. 
        /// </summary>
        IJobEntity CreateAsyncJob(IExecutionEntity execution, bool exclusive);

        /// <summary>
        /// Schedules and async job. If the <seealso cref="IAsyncExecutor"/> is running, it 
        /// can be executed immediately after the transaction. Otherwise it can
        /// be picked up by other executors.
        /// </summary>
        void ScheduleAsyncJob(IJobEntity job);

        /// <summary>
        /// Creates a <seealso cref="ITimerJobEntity"/> based on the current <seealso cref="IExecutionEntity"/> and the 
        /// configuration in the <seealso cref="TimerEventDefinition"/>. 
        /// </summary>
        ITimerJobEntity CreateTimerJob(TimerEventDefinition timerEventDefinition, bool interrupting, IExecutionEntity execution, string timerEventType, string jobHandlerConfiguration);

        /// <summary>
        /// Schedules a timer, meaning it will be inserted in the datastore.
        /// </summary>
        void ScheduleTimerJob(ITimerJobEntity timerJob);

        /// <summary>
        /// Moves a <seealso cref="ITimerJobEntity"/> to become an async <seealso cref="IJobEntity"/>. 
        /// 
        /// This happens for example when the due date of a timer is reached, 
        /// the timer entity then becomes a 'regular' async job that can be 
        /// picked up by the <seealso cref="IAsyncExecutor"/>.
        /// </summary>
        IJobEntity MoveTimerJobToExecutableJob(ITimerJobEntity timerJob);

        /// <summary>
        /// Moves an <seealso cref="IModelEntity"/> to become a <seealso cref="ITimerJobEntity"/>.
        /// 
        /// This happens for example when an async job is executed and fails.
        /// It then becomes a timer, as it needs to be retried later.
        /// </summary>
        ITimerJobEntity MoveJobToTimerJob(IAbstractJobEntity job);

        /// <summary>
        /// Moves an <seealso cref="IModelEntity"/> to become a <seealso cref="ISuspendedJobEntity"/>,
        /// such that the <seealso cref="IAsyncExecutor"/> won't pick it up anymore for execution.
        /// </summary>
        ISuspendedJobEntity MoveJobToSuspendedJob(IAbstractJobEntity job);

        /// <summary>
        /// Transforms a <seealso cref="ISuspendedJobEntity"/> back to an <seealso cref="IModelEntity"/>
        /// (i.e. to what it was originally). The job will now again be able to
        /// picked up by the <seealso cref="IAsyncExecutor"/>. 
        /// </summary>
        IAbstractJobEntity ActivateSuspendedJob(ISuspendedJobEntity job);

        /// <summary>
        /// Transforms an <seealso cref="IModelEntity"/> to a <seealso cref="IDeadLetterJobEntity"/>.
        /// This means that the job has been tried a configurable amount of times,
        /// but kept failing.
        /// </summary>
        IDeadLetterJobEntity MoveJobToDeadLetterJob(IAbstractJobEntity job);

        /// <summary>
        /// Transforms a <seealso cref="IDeadLetterJobEntity"/> to a <seealso cref="IJobEntity"/>, thus
        /// making it executable again. Note that a 'retries' parameter needs to be passed,
        /// as the job got into the deadletter table because of it failed and retries became 0.
        /// </summary>
        IJobEntity MoveDeadLetterJobToExecutableJob(IDeadLetterJobEntity deadLetterJobEntity, int retries);

        /// <summary>
        /// The ProcessEngineCongiguration instance will be passed when the <seealso cref="IProcessEngine"/> is built.
        /// </summary>
        ProcessEngineConfigurationImpl ProcessEngineConfiguration { set; }
    }
}