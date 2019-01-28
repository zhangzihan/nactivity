using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl.asyncexecutor
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.calendar;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.el;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.jobexecutor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.runtime;
    using Sys;
    using System.Globalization;

    public class DefaultJobManager : IJobManager
    {
        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;

        private readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<DefaultJobManager>();

        public DefaultJobManager()
        {
        }

        public DefaultJobManager(ProcessEngineConfigurationImpl processEngineConfiguration)
        {
            this.processEngineConfiguration = processEngineConfiguration;
        }

        public virtual IJobEntity createAsyncJob(IExecutionEntity execution, bool exclusive)
        {
            IJobEntity jobEntity = null;
            // When the async executor is activated, the job is directly passed on to the async executor thread
            if (AsyncExecutorActive)
            {
                jobEntity = internalCreateLockedAsyncJob(execution, exclusive);

            }
            else
            {
                jobEntity = internalCreateAsyncJob(execution, exclusive);
            }

            return jobEntity;
        }

        public virtual void scheduleAsyncJob(IJobEntity jobEntity)
        {
            processEngineConfiguration.JobEntityManager.insert(jobEntity);
            triggerExecutorIfNeeded(jobEntity);
        }

        protected internal virtual void triggerExecutorIfNeeded(IJobEntity jobEntity)
        {
            // When the async executor is activated, the job is directly passed on to the async executor thread
            if (AsyncExecutorActive)
            {
                hintAsyncExecutor(jobEntity);
            }
        }

        public virtual ITimerJobEntity createTimerJob(TimerEventDefinition timerEventDefinition, bool interrupting, IExecutionEntity execution, string timerEventType, string jobHandlerConfiguration)
        {

            ITimerJobEntity timerEntity = TimerUtil.createTimerEntityForTimerEventDefinition(timerEventDefinition, interrupting, execution, timerEventType, jobHandlerConfiguration);

            return timerEntity;
        }

        public virtual void scheduleTimerJob(ITimerJobEntity timerJob)
        {
            if (timerJob == null)
            {
                throw new ActivitiException("Empty timer job can not be scheduled");
            }

            processEngineConfiguration.TimerJobEntityManager.insert(timerJob);

            ICommandContext commandContext = Context.CommandContext;
            IActivitiEventDispatcher eventDispatcher = commandContext.EventDispatcher;
            if (eventDispatcher.Enabled)
            {
                eventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.TIMER_SCHEDULED, timerJob));
            }
        }

        public virtual IJobEntity moveTimerJobToExecutableJob(ITimerJobEntity timerJob)
        {
            if (timerJob == null)
            {
                throw new ActivitiException("Empty timer job can not be scheduled");
            }

            IJobEntity executableJob = createExecutableJobFromOtherJob(timerJob);
            bool insertSuccesful = processEngineConfiguration.JobEntityManager.insertJobEntity(executableJob);
            if (insertSuccesful)
            {
                processEngineConfiguration.TimerJobEntityManager.delete(timerJob);
                triggerExecutorIfNeeded(executableJob);
                return executableJob;
            }
            return null;
        }

        public virtual ITimerJobEntity moveJobToTimerJob(IAbstractJobEntity job)
        {
            ITimerJobEntity timerJob = createTimerJobFromOtherJob(job);
            bool insertSuccesful = processEngineConfiguration.TimerJobEntityManager.insertTimerJobEntity(timerJob);
            if (insertSuccesful)
            {
                if (job is IJobEntity)
                {
                    processEngineConfiguration.JobEntityManager.delete((IJobEntity)job);
                }
                else if (job is ISuspendedJobEntity)
                {
                    processEngineConfiguration.SuspendedJobEntityManager.delete((ISuspendedJobEntity)job);
                }

                return timerJob;
            }
            return null;
        }

        public virtual ISuspendedJobEntity moveJobToSuspendedJob(IAbstractJobEntity job)
        {
            ISuspendedJobEntity suspendedJob = createSuspendedJobFromOtherJob(job);
            processEngineConfiguration.SuspendedJobEntityManager.insert(suspendedJob);
            if (job is ITimerJobEntity)
            {
                processEngineConfiguration.TimerJobEntityManager.delete((ITimerJobEntity)job);

            }
            else if (job is IJobEntity)
            {
                processEngineConfiguration.JobEntityManager.delete((IJobEntity)job);
            }

            return suspendedJob;
        }

        public virtual IAbstractJobEntity activateSuspendedJob(ISuspendedJobEntity job)
        {
            IAbstractJobEntity activatedJob = null;
            if (Job_Fields.JOB_TYPE_TIMER.Equals(job.JobType))
            {
                activatedJob = createTimerJobFromOtherJob(job);
                processEngineConfiguration.TimerJobEntityManager.insert((ITimerJobEntity)activatedJob);

            }
            else
            {
                activatedJob = createExecutableJobFromOtherJob(job);
                IJobEntity jobEntity = (IJobEntity)activatedJob;
                processEngineConfiguration.JobEntityManager.insert(jobEntity);
                triggerExecutorIfNeeded(jobEntity);
            }

            processEngineConfiguration.SuspendedJobEntityManager.delete(job);
            return activatedJob;
        }

        public virtual IDeadLetterJobEntity moveJobToDeadLetterJob(IAbstractJobEntity job)
        {
            IDeadLetterJobEntity deadLetterJob = createDeadLetterJobFromOtherJob(job);
            processEngineConfiguration.DeadLetterJobEntityManager.insert(deadLetterJob);
            if (job is ITimerJobEntity)
            {
                processEngineConfiguration.TimerJobEntityManager.delete((ITimerJobEntity)job);

            }
            else if (job is IJobEntity)
            {
                processEngineConfiguration.JobEntityManager.delete((IJobEntity)job);
            }

            return deadLetterJob;
        }

        public virtual IJobEntity moveDeadLetterJobToExecutableJob(IDeadLetterJobEntity deadLetterJobEntity, int retries)
        {
            if (deadLetterJobEntity == null)
            {
                throw new ActivitiIllegalArgumentException("Null job provided");
            }

            IJobEntity executableJob = createExecutableJobFromOtherJob(deadLetterJobEntity);
            executableJob.Retries = retries;
            bool insertSuccesful = processEngineConfiguration.JobEntityManager.insertJobEntity(executableJob);
            if (insertSuccesful)
            {
                processEngineConfiguration.DeadLetterJobEntityManager.delete(deadLetterJobEntity);
                triggerExecutorIfNeeded(executableJob);
                return executableJob;
            }
            return null;
        }

        public virtual void execute(IJob job)
        {
            if (job is IJobEntity)
            {
                if (Job_Fields.JOB_TYPE_MESSAGE.Equals(job.JobType))
                {
                    executeMessageJob((IJobEntity)job);
                }
                else if (Job_Fields.JOB_TYPE_TIMER.Equals(job.JobType))
                {
                    executeTimerJob((IJobEntity)job);
                }

            }
            else
            {
                throw new ActivitiException("Only jobs with type JobEntity are supported to be executed");
            }
        }

        public virtual void unacquire(IJob job)
        {
            if (job == null)
            {
                return;
            }

            // Deleting the old job and inserting it again with another id,
            // will avoid that the job is immediately is picked up again (for example
            // when doing lots of exclusive jobs for the same process instance)
            if (job is IJobEntity)
            {
                IJobEntity jobEntity = (IJobEntity)job;
                processEngineConfiguration.JobEntityManager.delete(new KeyValuePair<string, object>("id", jobEntity.Id));

                IJobEntity newJobEntity = processEngineConfiguration.JobEntityManager.create();
                copyJobInfo(newJobEntity, jobEntity);
                newJobEntity.Id = null; // We want a new id to be assigned to this job
                newJobEntity.LockExpirationTime = null;
                newJobEntity.LockOwner = null;
                processEngineConfiguration.JobEntityManager.insert(newJobEntity);

                // We're not calling triggerExecutorIfNeeded here after the inser. The unacquire happened
                // for a reason (eg queue full or exclusive lock failure). No need to try it immediately again,
                // as the chance of failure will be high.

            }
            else
            {
                // It could be a v5 job, so simply unlock it.
                processEngineConfiguration.JobEntityManager.resetExpiredJob(job.Id);
            }

        }

        protected internal virtual void executeMessageJob(IJobEntity jobEntity)
        {
            executeJobHandler(jobEntity);
            if (!ReferenceEquals(jobEntity.Id, null))
            {
                Context.CommandContext.JobEntityManager.delete(jobEntity);
            }
        }

        protected internal virtual void executeTimerJob(IJobEntity timerEntity)
        {
            ITimerJobEntityManager timerJobEntityManager = processEngineConfiguration.TimerJobEntityManager;

            IVariableScope variableScope = null;
            if (!ReferenceEquals(timerEntity.ExecutionId, null))
            {
                variableScope = ExecutionEntityManager.findById<VariableScopeImpl>(new KeyValuePair<string, object>("id", timerEntity.ExecutionId));
            }

            if (variableScope == null)
            {
                variableScope = NoExecutionVariableScope.SharedInstance;
            }

            // set endDate if it was set to the definition
            restoreExtraData(timerEntity, variableScope);

            if (timerEntity.Duedate != null && !isValidTime(timerEntity, timerEntity.Duedate.GetValueOrDefault(DateTime.Now), variableScope))
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug($"Timer {timerEntity.Id} fired. but the dueDate is after the endDate.  Deleting timer.");
                }
                processEngineConfiguration.JobEntityManager.delete(timerEntity);
                return;
            }

            executeJobHandler(timerEntity);
            processEngineConfiguration.JobEntityManager.delete(timerEntity);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Timer {timerEntity.Id} fired. Deleting timer.");
            }

            if (!ReferenceEquals(timerEntity.Repeat, null))
            {
                ITimerJobEntity newTimerJobEntity = timerJobEntityManager.createAndCalculateNextTimer(timerEntity, variableScope);
                if (newTimerJobEntity != null)
                {
                    scheduleTimerJob(newTimerJobEntity);
                }
            }
        }

        protected internal virtual void executeJobHandler(IJobEntity jobEntity)
        {
            IExecutionEntity execution = null;
            if (!ReferenceEquals(jobEntity.ExecutionId, null))
            {
                execution = ExecutionEntityManager.findById<ExecutionEntityImpl>(new KeyValuePair<string, object>("id", jobEntity.ExecutionId));
            }

            IDictionary<string, IJobHandler> jobHandlers = processEngineConfiguration.JobHandlers;
            IJobHandler jobHandler = jobHandlers[jobEntity.JobHandlerType];
            jobHandler.execute(jobEntity, jobEntity.JobHandlerConfiguration, execution, CommandContext);
        }

        protected internal virtual void restoreExtraData(IJobEntity timerEntity, IVariableScope variableScope)
        {
            string activityId = timerEntity.JobHandlerConfiguration;

            if (timerEntity.JobHandlerType.Equals(TimerStartEventJobHandler.TYPE, StringComparison.CurrentCultureIgnoreCase) || timerEntity.JobHandlerType.Equals(TriggerTimerEventJobHandler.TYPE, StringComparison.CurrentCultureIgnoreCase))
            {

                activityId = TimerEventHandler.getActivityIdFromConfiguration(timerEntity.JobHandlerConfiguration);
                string endDateExpressionString = TimerEventHandler.getEndDateFromConfiguration(timerEntity.JobHandlerConfiguration);

                if (!ReferenceEquals(endDateExpressionString, null))
                {
                    IExpression endDateExpression = processEngineConfiguration.ExpressionManager.createExpression(endDateExpressionString);

                    string endDateString = null;

                    IBusinessCalendar businessCalendar = processEngineConfiguration.BusinessCalendarManager.getBusinessCalendar(getBusinessCalendarName(TimerEventHandler.geCalendarNameFromConfiguration(timerEntity.JobHandlerConfiguration), variableScope));

                    if (endDateExpression != null)
                    {
                        object endDateValue = endDateExpression.getValue(variableScope);
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
                            timerEntity.EndDate = businessCalendar.resolveEndDate(endDateString).GetValueOrDefault();
                        }
                    }
                }
            }

            int maxIterations = 1;
            if (!ReferenceEquals(timerEntity.ProcessDefinitionId, null))
            {
                Process process = ProcessDefinitionUtil.getProcess(timerEntity.ProcessDefinitionId);
                maxIterations = getMaxIterations(process, activityId);
                if (maxIterations <= 1)
                {
                    maxIterations = getMaxIterations(process, activityId);
                }
            }
            timerEntity.MaxIterations = maxIterations;
        }

        protected internal virtual int getMaxIterations(Process process, string activityId)
        {
            FlowElement flowElement = process.getFlowElement(activityId, true);
            if (flowElement != null)
            {
                if (flowElement is Event)
                {

                    Event @event = (Event)flowElement;
                    IList<EventDefinition> eventDefinitions = @event.EventDefinitions;

                    if (eventDefinitions != null)
                    {

                        foreach (EventDefinition eventDefinition in eventDefinitions)
                        {
                            if (eventDefinition is TimerEventDefinition)
                            {
                                TimerEventDefinition timerEventDefinition = (TimerEventDefinition)eventDefinition;
                                if (!ReferenceEquals(timerEventDefinition.TimeCycle, null))
                                {
                                    return calculateMaxIterationsValue(timerEventDefinition.TimeCycle);
                                }
                            }
                        }

                    }

                }
            }
            return -1;
        }

        protected internal virtual int calculateMaxIterationsValue(string originalExpression)
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

        protected internal virtual bool isValidTime(IJobEntity timerEntity, DateTime newTimerDate, IVariableScope variableScope)
        {
            IBusinessCalendar businessCalendar = processEngineConfiguration.BusinessCalendarManager.getBusinessCalendar(getBusinessCalendarName(TimerEventHandler.geCalendarNameFromConfiguration(timerEntity.JobHandlerConfiguration), variableScope));
            return businessCalendar.validateDuedate(timerEntity.Repeat, timerEntity.MaxIterations, timerEntity.EndDate, newTimerDate).Value;
        }

        protected internal virtual string getBusinessCalendarName(string calendarName, IVariableScope variableScope)
        {
            string businessCalendarName = CycleBusinessCalendar.NAME;
            if (!string.IsNullOrWhiteSpace(calendarName))
            {
                businessCalendarName = (string)Context.ProcessEngineConfiguration.ExpressionManager.createExpression(calendarName).getValue(variableScope);
            }
            return businessCalendarName;
        }

        protected internal virtual void hintAsyncExecutor(IJobEntity job)
        {
            AsyncJobAddedNotification jobAddedNotification = new AsyncJobAddedNotification(job, AsyncExecutor);
            CommandContext.addCloseListener(jobAddedNotification);
        }

        protected internal virtual IJobEntity internalCreateAsyncJob(IExecutionEntity execution, bool exclusive)
        {
            IJobEntity asyncJob = processEngineConfiguration.JobEntityManager.create();
            fillDefaultAsyncJobInfo(asyncJob, execution, exclusive);
            return asyncJob;
        }

        protected internal virtual IJobEntity internalCreateLockedAsyncJob(IExecutionEntity execution, bool exclusive)
        {
            IJobEntity asyncJob = processEngineConfiguration.JobEntityManager.create();
            fillDefaultAsyncJobInfo(asyncJob, execution, exclusive);

            //GregorianCalendar gregorianCalendar = new GregorianCalendar();
            //gregorianCalendar.Time = processEngineConfiguration.Clock.CurrentTime;
            //gregorianCalendar.add(DateTime.MILLISECOND, AsyncExecutor.AsyncJobLockTimeInMillis);
            //asyncJob.LockExpirationTime = gregorianCalendar.Time;
            asyncJob.LockOwner = AsyncExecutor.LockOwner;

            return asyncJob;
        }

        protected internal virtual void fillDefaultAsyncJobInfo(IJobEntity jobEntity, IExecutionEntity execution, bool exclusive)
        {
            jobEntity.JobType = Job_Fields.JOB_TYPE_MESSAGE;
            jobEntity.Revision = 1;
            jobEntity.Retries = processEngineConfiguration.AsyncExecutorNumberOfRetries;
            jobEntity.ExecutionId = execution.Id;
            jobEntity.ProcessInstanceId = execution.ProcessInstanceId;
            jobEntity.ProcessDefinitionId = execution.ProcessDefinitionId;
            jobEntity.Exclusive = exclusive;
            jobEntity.JobHandlerType = AsyncContinuationJobHandler.TYPE;

            // Inherit tenant id (if applicable)
            if (!ReferenceEquals(execution.TenantId, null))
            {
                jobEntity.TenantId = execution.TenantId;
            }
        }

        protected internal virtual IJobEntity createExecutableJobFromOtherJob(IAbstractJobEntity job)
        {
            IJobEntity executableJob = processEngineConfiguration.JobEntityManager.create();
            copyJobInfo(executableJob, job);

            if (AsyncExecutorActive)
            {
                GregorianCalendar gregorianCalendar = new GregorianCalendar();
                //gregorianCalendar.Time = processEngineConfiguration.Clock.CurrentTime;
                //gregorianCalendar.add(DateTime.MILLISECOND, AsyncExecutor.TimerLockTimeInMillis);
                //executableJob.LockExpirationTime = gregorianCalendar.Time;
                executableJob.LockOwner = AsyncExecutor.LockOwner;
            }

            return executableJob;
        }

        protected internal virtual ITimerJobEntity createTimerJobFromOtherJob(IAbstractJobEntity otherJob)
        {
            ITimerJobEntity timerJob = processEngineConfiguration.TimerJobEntityManager.create();
            copyJobInfo(timerJob, otherJob);
            return timerJob;
        }

        protected internal virtual ISuspendedJobEntity createSuspendedJobFromOtherJob(IAbstractJobEntity otherJob)
        {
            ISuspendedJobEntity suspendedJob = processEngineConfiguration.SuspendedJobEntityManager.create();
            copyJobInfo(suspendedJob, otherJob);
            return suspendedJob;
        }

        protected internal virtual IDeadLetterJobEntity createDeadLetterJobFromOtherJob(IAbstractJobEntity otherJob)
        {
            IDeadLetterJobEntity deadLetterJob = processEngineConfiguration.DeadLetterJobEntityManager.create();
            copyJobInfo(deadLetterJob, otherJob);
            return deadLetterJob;
        }

        protected internal virtual IAbstractJobEntity copyJobInfo(IAbstractJobEntity copyToJob, IAbstractJobEntity copyFromJob)
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


        protected internal virtual bool AsyncExecutorActive
        {
            get
            {
                return processEngineConfiguration.AsyncExecutor.Active;
            }
        }

        protected internal virtual ICommandContext CommandContext
        {
            get
            {
                return Context.CommandContext;
            }
        }

        protected internal virtual IAsyncExecutor AsyncExecutor
        {
            get
            {
                return processEngineConfiguration.AsyncExecutor;
            }
        }

        protected internal virtual IExecutionEntityManager ExecutionEntityManager
        {
            get
            {
                return processEngineConfiguration.ExecutionEntityManager;
            }
        }

    }

}