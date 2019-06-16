using System;

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
namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Calendars;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow;
    using System.Collections.Generic;

    /// 
    /// 

    public class JobRetryCmd : ICommand<object>
    {
        protected internal string jobId;
        protected internal Exception exception;

        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<JobRetryCmd>();

        public JobRetryCmd(string jobId, Exception exception)
        {
            this.jobId = jobId;
            this.exception = exception;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            IJobEntity job = commandContext.JobEntityManager.FindById<IJobEntity>(new KeyValuePair<string, object>("id", jobId));
            if (job == null)
            {
                return null;
            }

            ProcessEngineConfiguration processEngineConfig = commandContext.ProcessEngineConfiguration;

            IExecutionEntity executionEntity = fetchExecutionEntity(commandContext, job.ExecutionId);
            FlowElement currentFlowElement = executionEntity != null ? executionEntity.CurrentFlowElement : null;

            string failedJobRetryTimeCycleValue = null;
            if (currentFlowElement is ServiceTask)
            {
                failedJobRetryTimeCycleValue = ((ServiceTask)currentFlowElement).FailedJobRetryTimeCycleValue;
            }

            IAbstractJobEntity newJobEntity = null;
            if (currentFlowElement == null || ReferenceEquals(failedJobRetryTimeCycleValue, null))
            {

                log.LogDebug("activity or FailedJobRetryTimerCycleValue is null in job " + jobId + ". only decrementing retries.");

                if (job.Retries <= 1)
                {
                    newJobEntity = commandContext.JobManager.MoveJobToDeadLetterJob(job);
                }
                else
                {
                    newJobEntity = commandContext.JobManager.MoveJobToTimerJob(job);
                }

                newJobEntity.Retries = job.Retries - 1;
                if (!job.Duedate.HasValue || JobFields.JOB_TYPE_MESSAGE.Equals(job.JobType))
                {
                    // add wait time for failed async job
                    newJobEntity.Duedate = calculateDueDate(commandContext, processEngineConfig.AsyncFailedJobWaitTime, null);
                }
                else
                {
                    // add default wait time for failed job
                    newJobEntity.Duedate = calculateDueDate(commandContext, processEngineConfig.DefaultFailedJobWaitTime, job.Duedate);
                }

            }
            else
            {
                try
                {
                    DurationHelper durationHelper = new DurationHelper(failedJobRetryTimeCycleValue, processEngineConfig.Clock);
                    int jobRetries = job.Retries;
                    if (ReferenceEquals(job.ExceptionMessage, null))
                    {
                        // change default retries to the ones configured
                        jobRetries = durationHelper.Times;
                    }

                    if (jobRetries <= 1)
                    {
                        newJobEntity = commandContext.JobManager.MoveJobToDeadLetterJob(job);
                    }
                    else
                    {
                        newJobEntity = commandContext.JobManager.MoveJobToTimerJob(job);
                    }

                    newJobEntity.Duedate = durationHelper.DateAfter;

                    if (ReferenceEquals(job.ExceptionMessage, null))
                    { 
                        // is it the first exception
                        log.LogDebug("Applying JobRetryStrategy '" + failedJobRetryTimeCycleValue + "' the first time for job " + job.Id + " with " + durationHelper.Times + " retries");

                    }
                    else
                    {
                        log.LogDebug("Decrementing retries of JobRetryStrategy '" + failedJobRetryTimeCycleValue + "' for job " + job.Id);
                    }

                    newJobEntity.Retries = jobRetries - 1;

                }
                catch (Exception)
                {
                    throw new ActivitiException("failedJobRetryTimeCylcle has wrong format:" + failedJobRetryTimeCycleValue, exception);
                }
            }

            if (exception != null)
            {
                newJobEntity.ExceptionMessage = exception.Message;
                newJobEntity.ExceptionStacktrace = ExceptionStacktrace;
            }

            // Dispatch both an update and a retry-decrement event
            IActivitiEventDispatcher eventDispatcher = commandContext.EventDispatcher;
            if (eventDispatcher.Enabled)
            {
                eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_UPDATED, newJobEntity));
                eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_RETRIES_DECREMENTED, newJobEntity));
            }

            return null;
        }

        protected internal virtual DateTime calculateDueDate(ICommandContext commandContext, int waitTimeInSeconds, DateTime? oldDate)
        {
            DateTime newDateCal = DateTime.Now;
            if (oldDate != null)
            {
                newDateCal = new DateTime(oldDate.Value.Ticks);

            }
            else
            {
                newDateCal = new DateTime(commandContext.ProcessEngineConfiguration.Clock.CurrentTime.Ticks);
            }

            newDateCal.AddSeconds(waitTimeInSeconds);
            return newDateCal;
        }

        protected internal virtual string ExceptionStacktrace
        {
            get
            {
                Console.WriteLine(exception.StackTrace);
                //StringWriter stringWriter = new StringWriter();
                //exception.printStackTrace(new PrintWriter(stringWriter));
                return exception.StackTrace;
            }
        }

        protected internal virtual IExecutionEntity fetchExecutionEntity(ICommandContext commandContext, string executionId)
        {
            if (ReferenceEquals(executionId, null))
            {
                return null;
            }
            return commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(executionId);
        }

    }

}