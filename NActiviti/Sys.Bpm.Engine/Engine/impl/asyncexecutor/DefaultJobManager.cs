using System;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Asyncexecutor
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Calendars;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.EL;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.JobExecutors;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow;
    using System.Globalization;

    /// <summary>
    /// 
    /// </summary>
    public class DefaultJobManager : IJobManager
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;

        private readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<DefaultJobManager>();

        /// <summary>
        /// 
        /// </summary>
        public DefaultJobManager()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngineConfiguration"></param>
        public DefaultJobManager(ProcessEngineConfigurationImpl processEngineConfiguration)
        {
            this.processEngineConfiguration = processEngineConfiguration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="exclusive"></param>
        /// <returns></returns>
        public virtual IJobEntity CreateAsyncJob(IExecutionEntity execution, bool exclusive)
        {
            IJobEntity jobEntity;
            // When the async executor is activated, the job is directly passed on to the async executor thread
            if (AsyncExecutorActive)
            {
                jobEntity = InternalCreateLockedAsyncJob(execution, exclusive);

            }
            else
            {
                jobEntity = InternalCreateAsyncJob(execution, exclusive);
            }

            return jobEntity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobEntity"></param>
        public virtual void ScheduleAsyncJob(IJobEntity jobEntity)
        {
            processEngineConfiguration.JobEntityManager.Insert(jobEntity);
            TriggerExecutorIfNeeded(jobEntity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobEntity"></param>
        protected internal virtual void TriggerExecutorIfNeeded(IJobEntity jobEntity)
        {
            // When the async executor is activated, the job is directly passed on to the async executor thread
            if (AsyncExecutorActive)
            {
                HintAsyncExecutor(jobEntity);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timerEventDefinition"></param>
        /// <param name="interrupting"></param>
        /// <param name="execution"></param>
        /// <param name="timerEventType"></param>
        /// <param name="jobHandlerConfiguration"></param>
        /// <returns></returns>
        public virtual ITimerJobEntity CreateTimerJob(TimerEventDefinition timerEventDefinition, bool interrupting, IExecutionEntity execution, string timerEventType, string jobHandlerConfiguration)
        {

            ITimerJobEntity timerEntity = TimerUtil.CreateTimerEntityForTimerEventDefinition(timerEventDefinition, interrupting, execution, timerEventType, jobHandlerConfiguration);

            return timerEntity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timerJob"></param>
        public virtual void ScheduleTimerJob(ITimerJobEntity timerJob)
        {
            if (timerJob == null)
            {
                throw new ActivitiException("Empty timer job can not be scheduled");
            }

            processEngineConfiguration.TimerJobEntityManager.Insert(timerJob);

            ICommandContext commandContext = Context.CommandContext;
            IActivitiEventDispatcher eventDispatcher = commandContext.EventDispatcher;
            if (eventDispatcher.Enabled)
            {
                eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.TIMER_SCHEDULED, timerJob));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timerJob"></param>
        /// <returns></returns>
        public virtual IJobEntity MoveTimerJobToExecutableJob(ITimerJobEntity timerJob)
        {
            if (timerJob == null)
            {
                throw new ActivitiException("Empty timer job can not be scheduled");
            }

            IJobEntity executableJob = CreateExecutableJobFromOtherJob(timerJob);
            bool insertSuccesful = processEngineConfiguration.JobEntityManager.InsertJobEntity(executableJob);
            if (insertSuccesful)
            {
                processEngineConfiguration.TimerJobEntityManager.Delete(timerJob);
                TriggerExecutorIfNeeded(executableJob);
                return executableJob;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public virtual ITimerJobEntity MoveJobToTimerJob(IAbstractJobEntity job)
        {
            ITimerJobEntity timerJob = CreateTimerJobFromOtherJob(job);
            bool insertSuccesful = processEngineConfiguration.TimerJobEntityManager.InsertTimerJobEntity(timerJob);
            if (insertSuccesful)
            {
                if (job is IJobEntity)
                {
                    processEngineConfiguration.JobEntityManager.Delete((IJobEntity)job);
                }
                else if (job is ISuspendedJobEntity)
                {
                    processEngineConfiguration.SuspendedJobEntityManager.Delete((ISuspendedJobEntity)job);
                }

                return timerJob;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public virtual ISuspendedJobEntity MoveJobToSuspendedJob(IAbstractJobEntity job)
        {
            ISuspendedJobEntity suspendedJob = CreateSuspendedJobFromOtherJob(job);
            processEngineConfiguration.SuspendedJobEntityManager.Insert(suspendedJob);
            if (job is ITimerJobEntity)
            {
                processEngineConfiguration.TimerJobEntityManager.Delete((ITimerJobEntity)job);

            }
            else if (job is IJobEntity)
            {
                processEngineConfiguration.JobEntityManager.Delete((IJobEntity)job);
            }

            return suspendedJob;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public virtual IAbstractJobEntity ActivateSuspendedJob(ISuspendedJobEntity job)
        {
            IAbstractJobEntity activatedJob;
            if (JobFields.JOB_TYPE_TIMER.Equals(job.JobType))
            {
                activatedJob = CreateTimerJobFromOtherJob(job);
                processEngineConfiguration.TimerJobEntityManager.Insert((ITimerJobEntity)activatedJob);
            }
            else
            {
                activatedJob = CreateExecutableJobFromOtherJob(job);
                IJobEntity jobEntity = (IJobEntity)activatedJob;
                processEngineConfiguration.JobEntityManager.Insert(jobEntity);
                TriggerExecutorIfNeeded(jobEntity);
            }

            processEngineConfiguration.SuspendedJobEntityManager.Delete(job);
            return activatedJob;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public virtual IDeadLetterJobEntity MoveJobToDeadLetterJob(IAbstractJobEntity job)
        {
            IDeadLetterJobEntity deadLetterJob = CreateDeadLetterJobFromOtherJob(job);
            processEngineConfiguration.DeadLetterJobEntityManager.Insert(deadLetterJob);
            if (job is ITimerJobEntity)
            {
                processEngineConfiguration.TimerJobEntityManager.Delete((ITimerJobEntity)job);

            }
            else if (job is IJobEntity)
            {
                processEngineConfiguration.JobEntityManager.Delete((IJobEntity)job);
            }

            return deadLetterJob;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deadLetterJobEntity"></param>
        /// <param name="retries"></param>
        /// <returns></returns>
        public virtual IJobEntity MoveDeadLetterJobToExecutableJob(IDeadLetterJobEntity deadLetterJobEntity, int retries)
        {
            if (deadLetterJobEntity == null)
            {
                throw new ActivitiIllegalArgumentException("Null job provided");
            }

            IJobEntity executableJob = CreateExecutableJobFromOtherJob(deadLetterJobEntity);
            executableJob.Retries = retries;
            bool insertSuccesful = processEngineConfiguration.JobEntityManager.InsertJobEntity(executableJob);
            if (insertSuccesful)
            {
                processEngineConfiguration.DeadLetterJobEntityManager.Delete(deadLetterJobEntity);
                TriggerExecutorIfNeeded(executableJob);
                return executableJob;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        public virtual void Execute(IJob job)
        {
            if (job is IJobEntity)
            {
                if (JobFields.JOB_TYPE_MESSAGE.Equals(job.JobType))
                {
                    ExecuteMessageJob((IJobEntity)job);
                }
                else if (JobFields.JOB_TYPE_TIMER.Equals(job.JobType))
                {
                    ExecuteTimerJob((IJobEntity)job);
                }

            }
            else
            {
                throw new ActivitiException("Only jobs with type JobEntity are supported to be executed");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        public virtual void Unacquire(IJob job)
        {
            if (job == null)
            {
                return;
            }

            // Deleting the old job and inserting it again with another id,
            // will avoid that the job is immediately is picked up again (for example
            // when doing lots of exclusive jobs for the same process instance)
            if (job is IJobEntity jobEntity)
            {
                processEngineConfiguration.JobEntityManager.Delete(new KeyValuePair<string, object>("id", jobEntity.Id));

                IJobEntity newJobEntity = processEngineConfiguration.JobEntityManager.Create();
                CopyJobInfo(newJobEntity, jobEntity);
                newJobEntity.Id = null; // We want a new id to be assigned to this job
                newJobEntity.LockExpirationTime = null;
                newJobEntity.LockOwner = null;
                processEngineConfiguration.JobEntityManager.Insert(newJobEntity);

                // We're not calling triggerExecutorIfNeeded here after the inser. The unacquire happened
                // for a reason (eg queue full or exclusive lock failure). No need to try it immediately again,
                // as the chance of failure will be high.

            }
            else
            {
                // It could be a v5 job, so simply unlock it.
                processEngineConfiguration.JobEntityManager.ResetExpiredJob(job.Id);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobEntity"></param>
        protected internal virtual void ExecuteMessageJob(IJobEntity jobEntity)
        {
            ExecuteJobHandler(jobEntity);
            if (!(jobEntity.Id is null))
            {
                Context.CommandContext.JobEntityManager.Delete(jobEntity);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timerEntity"></param>
        protected internal virtual void ExecuteTimerJob(IJobEntity timerEntity)
        {
            ITimerJobEntityManager timerJobEntityManager = processEngineConfiguration.TimerJobEntityManager;

            IVariableScope variableScope = null;
            if (!(timerEntity.ExecutionId is null))
            {
                variableScope = ExecutionEntityManager.FindById<VariableScopeImpl>(timerEntity.ExecutionId);
            }

            if (variableScope == null)
            {
                variableScope = NoExecutionVariableScope.SharedInstance;
            }

            // set endDate if it was set to the definition
            RestoreExtraData(timerEntity, variableScope);

            if (timerEntity.Duedate != null && !IsValidTime(timerEntity, timerEntity.Duedate.GetValueOrDefault(DateTime.Now), variableScope))
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug($"Timer {timerEntity.Id} fired. but the dueDate is after the endDate.  Deleting timer.");
                }
                processEngineConfiguration.JobEntityManager.Delete(timerEntity);
                return;
            }

            ExecuteJobHandler(timerEntity);
            processEngineConfiguration.JobEntityManager.Delete(timerEntity);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Timer {timerEntity.Id} fired. Deleting timer.");
            }

            if (!(timerEntity.Repeat is null))
            {
                ITimerJobEntity newTimerJobEntity = timerJobEntityManager.CreateAndCalculateNextTimer(timerEntity, variableScope);
                if (newTimerJobEntity != null)
                {
                    ScheduleTimerJob(newTimerJobEntity);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobEntity"></param>
        protected internal virtual void ExecuteJobHandler(IJobEntity jobEntity)
        {
            IExecutionEntity execution = null;
            if (!(jobEntity.ExecutionId is null))
            {
                execution = ExecutionEntityManager.FindById<ExecutionEntityImpl>(jobEntity.ExecutionId);
            }

            IDictionary<string, IJobHandler> jobHandlers = processEngineConfiguration.JobHandlers;
            IJobHandler jobHandler = jobHandlers[jobEntity.JobHandlerType];
            jobHandler.Execute(jobEntity, jobEntity.JobHandlerConfiguration, execution, CommandContext);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timerEntity"></param>
        /// <param name="variableScope"></param>
        protected internal virtual void RestoreExtraData(IJobEntity timerEntity, IVariableScope variableScope)
        {
            string activityId = timerEntity.JobHandlerConfiguration;

            if (timerEntity.JobHandlerType.Equals(TimerStartEventJobHandler.TYPE, StringComparison.CurrentCultureIgnoreCase) || timerEntity.JobHandlerType.Equals(TriggerTimerEventJobHandler.TYPE, StringComparison.CurrentCultureIgnoreCase))
            {
                activityId = TimerEventHandler.GetActivityIdFromConfiguration(timerEntity.JobHandlerConfiguration);
                string endDateExpressionString = TimerEventHandler.GetEndDateFromConfiguration(timerEntity.JobHandlerConfiguration);

                if (!(endDateExpressionString is null))
                {
                    IExpression endDateExpression = processEngineConfiguration.ExpressionManager.CreateExpression(endDateExpressionString);

                    string endDateString = null;

                    IBusinessCalendar businessCalendar = processEngineConfiguration.BusinessCalendarManager.GetBusinessCalendar(GetBusinessCalendarName(TimerEventHandler.GeCalendarNameFromConfiguration(timerEntity.JobHandlerConfiguration), variableScope));

                    if (endDateExpression != null)
                    {
                        object endDateValue = endDateExpression.GetValue(variableScope);
                        if (endDateValue is string)
                        {
                            endDateString = (string)endDateValue;
                        }
                        else if (endDateValue is DateTime)
                        {
                            timerEntity.EndDate = (DateTime)endDateValue;
                        }
                        else
                        {
                            throw new ActivitiException("Timer '" + ((IExecutionEntity)variableScope).ActivityId + "' was not configured with a valid duration/time, either hand in a java.util.Date or a String in format 'yyyy-MM-dd'T'hh:mm:ss'");
                        }

                        if (!timerEntity.EndDate.HasValue)
                        {
                            timerEntity.EndDate = businessCalendar.ResolveEndDate(endDateString).GetValueOrDefault();
                        }
                    }
                }
            }

            int maxIterations = 1;
            if (!(timerEntity.ProcessDefinitionId is null))
            {
                Process process = ProcessDefinitionUtil.GetProcess(timerEntity.ProcessDefinitionId);
                maxIterations = GetMaxIterations(process, activityId);
                if (maxIterations <= 1)
                {
                    maxIterations = GetMaxIterations(process, activityId);
                }
            }
            timerEntity.MaxIterations = maxIterations;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>
        protected internal virtual int GetMaxIterations(Process process, string activityId)
        {
            FlowElement flowElement = process.GetFlowElement(activityId, true);
            if (flowElement != null)
            {
                if (flowElement is Event @event)
                {
                    IList<EventDefinition> eventDefinitions = @event.EventDefinitions;

                    if (eventDefinitions != null)
                    {
                        foreach (EventDefinition eventDefinition in eventDefinitions)
                        {
                            if (eventDefinition is TimerEventDefinition timerEventDefinition)
                            {
                                if (!(timerEventDefinition.TimeCycle is null))
                                {
                                    return CalculateMaxIterationsValue(timerEventDefinition.TimeCycle);
                                }
                            }
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalExpression"></param>
        /// <returns></returns>
        protected internal virtual int CalculateMaxIterationsValue(string originalExpression)
        {
            int times = int.MaxValue;
            IList<string> expression = new List<string>(originalExpression.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
            if (expression.Count > 1 && expression[0].StartsWith("R", StringComparison.Ordinal))
            {
                times = int.MaxValue;
                if (expression[0].Length > 1)
                {
                    times = int.Parse(expression[0].Substring(1));
                }
            }
            return times;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timerEntity"></param>
        /// <param name="newTimerDate"></param>
        /// <param name="variableScope"></param>
        /// <returns></returns>
        protected internal virtual bool IsValidTime(IJobEntity timerEntity, DateTime newTimerDate, IVariableScope variableScope)
        {
            IBusinessCalendar businessCalendar = processEngineConfiguration.BusinessCalendarManager.GetBusinessCalendar(GetBusinessCalendarName(TimerEventHandler.GeCalendarNameFromConfiguration(timerEntity.JobHandlerConfiguration), variableScope));
            return businessCalendar.ValidateDuedate(timerEntity.Repeat, timerEntity.MaxIterations, timerEntity.EndDate, newTimerDate).Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calendarName"></param>
        /// <param name="variableScope"></param>
        /// <returns></returns>
        protected internal virtual string GetBusinessCalendarName(string calendarName, IVariableScope variableScope)
        {
            string businessCalendarName = CycleBusinessCalendar.NAME;
            if (!string.IsNullOrWhiteSpace(calendarName))
            {
                businessCalendarName = (string)Context.ProcessEngineConfiguration.ExpressionManager.CreateExpression(calendarName).GetValue(variableScope);
            }
            return businessCalendarName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        protected internal virtual void HintAsyncExecutor(IJobEntity job)
        {
            AsyncJobAddedNotification jobAddedNotification = new AsyncJobAddedNotification(job, AsyncExecutor);
            CommandContext.AddCloseListener(jobAddedNotification);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="exclusive"></param>
        /// <returns></returns>
        protected internal virtual IJobEntity InternalCreateAsyncJob(IExecutionEntity execution, bool exclusive)
        {
            IJobEntity asyncJob = processEngineConfiguration.JobEntityManager.Create();
            FillDefaultAsyncJobInfo(asyncJob, execution, exclusive);
            return asyncJob;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="exclusive"></param>
        /// <returns></returns>
        protected internal virtual IJobEntity InternalCreateLockedAsyncJob(IExecutionEntity execution, bool exclusive)
        {
            IJobEntity asyncJob = processEngineConfiguration.JobEntityManager.Create();
            FillDefaultAsyncJobInfo(asyncJob, execution, exclusive);

            //GregorianCalendar gregorianCalendar = new GregorianCalendar();
            //gregorianCalendar.Time = processEngineConfiguration.Clock.CurrentTime;
            //gregorianCalendar.add(DateTime.MILLISECOND, AsyncExecutor.AsyncJobLockTimeInMillis);
            //asyncJob.LockExpirationTime = gregorianCalendar.Time;

            //将job锁定为一定时间（默认30秒）
            asyncJob.LockExpirationTime = DateTime.Now.AddMilliseconds(AsyncExecutor.AsyncJobLockTimeInMillis);
            asyncJob.LockOwner = AsyncExecutor.LockOwner;

            return asyncJob;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobEntity"></param>
        /// <param name="execution"></param>
        /// <param name="exclusive"></param>
        protected internal virtual void FillDefaultAsyncJobInfo(IJobEntity jobEntity, IExecutionEntity execution, bool exclusive)
        {
            jobEntity.JobType = JobFields.JOB_TYPE_MESSAGE;
            jobEntity.Revision = 1;
            jobEntity.Retries = processEngineConfiguration.AsyncExecutorNumberOfRetries;
            jobEntity.ExecutionId = execution.Id;
            jobEntity.ProcessInstanceId = execution.ProcessInstanceId;
            jobEntity.ProcessDefinitionId = execution.ProcessDefinitionId;
            jobEntity.Exclusive = exclusive;
            jobEntity.JobHandlerType = AsyncContinuationJobHandler.TYPE;

            // Inherit tenant id (if applicable)
            if (!(execution.TenantId is null))
            {
                jobEntity.TenantId = execution.TenantId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        protected internal virtual IJobEntity CreateExecutableJobFromOtherJob(IAbstractJobEntity job)
        {
            IJobEntity executableJob = processEngineConfiguration.JobEntityManager.Create();
            CopyJobInfo(executableJob, job);

            if (AsyncExecutorActive)
            {
                //GregorianCalendar gregorianCalendar = new GregorianCalendar();
                //gregorianCalendar.Time = processEngineConfiguration.Clock.CurrentTime;
                //gregorianCalendar.add(DateTime.MILLISECOND, AsyncExecutor.TimerLockTimeInMillis);

                //将job锁定为一定时间（默认30秒）
                executableJob.LockExpirationTime = DateTime.Now.AddMilliseconds(AsyncExecutor.TimerLockTimeInMillis);
                executableJob.LockOwner = AsyncExecutor.LockOwner;
            }

            return executableJob;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherJob"></param>
        /// <returns></returns>
        protected internal virtual ITimerJobEntity CreateTimerJobFromOtherJob(IAbstractJobEntity otherJob)
        {
            ITimerJobEntity timerJob = processEngineConfiguration.TimerJobEntityManager.Create();
            CopyJobInfo(timerJob, otherJob);
            return timerJob;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherJob"></param>
        /// <returns></returns>
        protected internal virtual ISuspendedJobEntity CreateSuspendedJobFromOtherJob(IAbstractJobEntity otherJob)
        {
            ISuspendedJobEntity suspendedJob = processEngineConfiguration.SuspendedJobEntityManager.Create();
            CopyJobInfo(suspendedJob, otherJob);
            return suspendedJob;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherJob"></param>
        /// <returns></returns>
        protected internal virtual IDeadLetterJobEntity CreateDeadLetterJobFromOtherJob(IAbstractJobEntity otherJob)
        {
            IDeadLetterJobEntity deadLetterJob = processEngineConfiguration.DeadLetterJobEntityManager.Create();
            CopyJobInfo(deadLetterJob, otherJob);
            return deadLetterJob;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="copyToJob"></param>
        /// <param name="copyFromJob"></param>
        /// <returns></returns>
        protected internal virtual IAbstractJobEntity CopyJobInfo(IAbstractJobEntity copyToJob, IAbstractJobEntity copyFromJob)
        {
            copyToJob.Duedate = copyFromJob.Duedate;
            copyToJob.EndDate = copyFromJob.EndDate;
            copyToJob.Exclusive = copyFromJob.Exclusive;
            copyToJob.ExecutionId = copyFromJob.ExecutionId;
            copyToJob.Id = copyFromJob.Id;
            copyToJob.JobHandlerConfiguration = copyFromJob.JobHandlerConfiguration;
            copyToJob.JobHandlerType = copyFromJob.JobHandlerType;
            copyToJob.JobType = copyFromJob.JobType;
            copyToJob.ExceptionMessage = copyFromJob.ExceptionMessage;
            copyToJob.ExceptionStacktrace = copyFromJob.ExceptionStacktrace;
            copyToJob.MaxIterations = copyFromJob.MaxIterations;
            copyToJob.ProcessDefinitionId = copyFromJob.ProcessDefinitionId;
            copyToJob.ProcessInstanceId = copyFromJob.ProcessInstanceId;
            copyToJob.Repeat = copyFromJob.Repeat;
            copyToJob.Retries = copyFromJob.Retries;
            copyToJob.Revision = copyFromJob.Revision;
            copyToJob.TenantId = copyFromJob.TenantId;

            return copyToJob;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessEngineConfigurationImpl ProcessEngineConfiguration
        {
            get
            {
                return processEngineConfiguration;
            }
            set
            {
                this.processEngineConfiguration = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual bool AsyncExecutorActive
        {
            get
            {
                return processEngineConfiguration.AsyncExecutor.Active;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual ICommandContext CommandContext
        {
            get
            {
                return Context.CommandContext;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual IAsyncExecutor AsyncExecutor
        {
            get
            {
                return processEngineConfiguration.AsyncExecutor;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual IExecutionEntityManager ExecutionEntityManager
        {
            get
            {
                return processEngineConfiguration.ExecutionEntityManager;
            }
        }
    }
}