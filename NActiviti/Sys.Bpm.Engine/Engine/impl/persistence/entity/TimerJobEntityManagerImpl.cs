using System;
using System.Collections.Generic;
using System.Text;

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

namespace Sys.Workflow.engine.impl.persistence.entity
{

    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.calendar;
    using Sys.Workflow.engine.impl.cfg;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.jobexecutor;
    using Sys.Workflow.engine.impl.persistence.entity.data;
    using Sys.Workflow.engine.runtime;
    using System.Linq;

    public class TimerJobEntityManagerImpl : AbstractEntityManager<ITimerJobEntity>, ITimerJobEntityManager
    {

        protected internal ITimerJobDataManager jobDataManager;

        public TimerJobEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, ITimerJobDataManager jobDataManager) : base(processEngineConfiguration)
        {
            this.jobDataManager = jobDataManager;
        }

        public virtual ITimerJobEntity CreateAndCalculateNextTimer(IJobEntity timerEntity, IVariableScope variableScope)
        {
            int repeatValue = CalculateRepeatValue(timerEntity);
            if (repeatValue != 0)
            {
                if (repeatValue > 0)
                {
                    SetNewRepeat(timerEntity, repeatValue);
                }
                DateTime? newTimer = CalculateNextTimer(timerEntity, variableScope);
                if (newTimer != null && IsValidTime(timerEntity, newTimer, variableScope))
                {
                    ITimerJobEntity te = CreateTimer(timerEntity);
                    te.Duedate = newTimer;
                    return te;
                }
            }
            return null;
        }

        public virtual IList<ITimerJobEntity> FindTimerJobsToExecute(Page page)
        {
            return jobDataManager.FindTimerJobsToExecute(page);
        }

        public virtual IList<ITimerJobEntity> FindJobsByTypeAndProcessDefinitionId(string jobHandlerType, string processDefinitionId)
        {
            return jobDataManager.FindJobsByTypeAndProcessDefinitionId(jobHandlerType, processDefinitionId);
        }

        public virtual IList<ITimerJobEntity> FindJobsByTypeAndProcessDefinitionKeyNoTenantId(string jobHandlerType, string processDefinitionKey)
        {
            return jobDataManager.FindJobsByTypeAndProcessDefinitionKeyNoTenantId(jobHandlerType, processDefinitionKey);
        }

        public virtual IList<ITimerJobEntity> FindJobsByTypeAndProcessDefinitionKeyAndTenantId(string jobHandlerType, string processDefinitionKey, string tenantId)
        {
            return jobDataManager.FindJobsByTypeAndProcessDefinitionKeyAndTenantId(jobHandlerType, processDefinitionKey, tenantId);
        }

        public virtual IList<ITimerJobEntity> FindJobsByExecutionId(string id)
        {
            return jobDataManager.FindJobsByExecutionId(id);
        }

        public virtual IList<ITimerJobEntity> FindJobsByProcessInstanceId(string id)
        {
            return jobDataManager.FindJobsByProcessInstanceId(id);
        }

        public virtual IList<IJob> FindJobsByQueryCriteria(ITimerJobQuery jobQuery, Page page)
        {
            return jobDataManager.FindJobsByQueryCriteria(jobQuery, page);
        }

        public virtual long FindJobCountByQueryCriteria(ITimerJobQuery jobQuery)
        {
            return jobDataManager.FindJobCountByQueryCriteria(jobQuery);
        }

        public virtual void UpdateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            jobDataManager.UpdateJobTenantIdForDeployment(deploymentId, newTenantId);
        }

        public virtual bool InsertTimerJobEntity(ITimerJobEntity timerJobEntity)
        {
            return DoInsert(timerJobEntity, true);
        }

        public override void Insert(ITimerJobEntity jobEntity)
        {
            Insert(jobEntity, true);
        }

        public override void Insert(ITimerJobEntity jobEntity, bool fireCreateEvent)
        {
            DoInsert(jobEntity, fireCreateEvent);
        }

        protected internal virtual bool DoInsert(ITimerJobEntity jobEntity, bool fireCreateEvent)
        {
            // add link to execution
            if (!(jobEntity.ExecutionId is null))
            {
                IExecutionEntity execution = ExecutionEntityManager.FindById<IExecutionEntity>(jobEntity.ExecutionId);
                if (execution != null)
                {
                    execution.TimerJobs.Add(jobEntity);

                    // Inherit tenant if (if applicable)
                    if (!(execution.TenantId is null))
                    {
                        jobEntity.TenantId = execution.TenantId;
                    }

                    if (IsExecutionRelatedEntityCountEnabled(execution))
                    {
                        ICountingExecutionEntity countingExecutionEntity = (ICountingExecutionEntity)execution;
                        countingExecutionEntity.TimerJobCount += 1;
                    }
                }
                else
                {
                    // In case the job has an executionId, but the Execution is not found,
                    // it means that for example for a boundary timer event on a user task,
                    // the task has been completed and the Execution and job have been removed.
                    return false;
                }
            }

            base.Insert(jobEntity, fireCreateEvent);
            return true;
        }

        public override void Delete(ITimerJobEntity jobEntity)
        {
            base.Delete(jobEntity);

            DeleteExceptionByteArrayRef(jobEntity);
            RemoveExecutionLink(jobEntity);

            if (!(jobEntity.ExecutionId is null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = ExecutionEntityManager.FindById<ICountingExecutionEntity>(jobEntity.ExecutionId);
                if (IsExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.TimerJobCount -= 1;
                }
            }

            // Send event
            if (EventDispatcher.Enabled)
            {
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_DELETED, this));
            }
        }

        /// <summary>
        /// Removes the job's execution's reference to this job, if the job has an associated execution.
        /// Subclasses may override to provide custom implementations.
        /// </summary>
        protected internal virtual void RemoveExecutionLink(ITimerJobEntity jobEntity)
        {
            if (!(jobEntity.ExecutionId is null))
            {
                IExecutionEntity execution = ExecutionEntityManager.FindById<IExecutionEntity>(jobEntity.ExecutionId);
                if (execution != null)
                {
                    execution.TimerJobs.Remove(jobEntity);
                }
            }
        }

        /// <summary>
        /// Deletes a the byte array used to store the exception information.  Subclasses may override
        /// to provide custom implementations.
        /// </summary>
        protected internal virtual void DeleteExceptionByteArrayRef(ITimerJobEntity jobEntity)
        {
            if (jobEntity.ExceptionByteArrayRef is ByteArrayRef exceptionByteArrayRef)
            {
                exceptionByteArrayRef.Delete();
            }
        }

        protected internal virtual ITimerJobEntity CreateTimer(IJobEntity te)
        {
            ITimerJobEntity newTimerEntity = Create();
            newTimerEntity.JobHandlerConfiguration = te.JobHandlerConfiguration;
            newTimerEntity.JobHandlerType = te.JobHandlerType;
            newTimerEntity.Exclusive = te.Exclusive;
            newTimerEntity.Repeat = te.Repeat;
            newTimerEntity.Retries = te.Retries;
            newTimerEntity.EndDate = te.EndDate;
            newTimerEntity.ExecutionId = te.ExecutionId;
            newTimerEntity.ProcessInstanceId = te.ProcessInstanceId;
            newTimerEntity.ProcessDefinitionId = te.ProcessDefinitionId;

            // Inherit tenant
            newTimerEntity.TenantId = te.TenantId;
            newTimerEntity.JobType = JobFields.JOB_TYPE_TIMER;
            return newTimerEntity;
        }

        protected internal virtual void SetNewRepeat(IJobEntity timerEntity, int newRepeatValue)
        {
            IList<string> expression = new List<string>(timerEntity.Repeat.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
            expression = expression.Skip(1).Take(expression.Count).ToList();
            StringBuilder repeatBuilder = new StringBuilder("R");
            repeatBuilder.Append(newRepeatValue);
            foreach (string value in expression)
            {
                repeatBuilder.Append("/");
                repeatBuilder.Append(value);
            }
            timerEntity.Repeat = repeatBuilder.ToString();
        }

        protected internal virtual bool IsValidTime(IJobEntity timerEntity, DateTime? newTimerDate, IVariableScope variableScope)
        {
            IBusinessCalendar businessCalendar = ProcessEngineConfiguration.BusinessCalendarManager.GetBusinessCalendar(GetBusinessCalendarName(TimerEventHandler.GeCalendarNameFromConfiguration(timerEntity.JobHandlerConfiguration), variableScope));
            return businessCalendar.ValidateDuedate(timerEntity.Repeat, timerEntity.MaxIterations, timerEntity.EndDate, newTimerDate).Value;
        }

        protected internal virtual DateTime? CalculateNextTimer(IJobEntity timerEntity, IVariableScope variableScope)
        {
            IBusinessCalendar businessCalendar = ProcessEngineConfiguration.BusinessCalendarManager.GetBusinessCalendar(GetBusinessCalendarName(TimerEventHandler.GeCalendarNameFromConfiguration(timerEntity.JobHandlerConfiguration), variableScope));
            return businessCalendar.ResolveDuedate(timerEntity.Repeat, timerEntity.MaxIterations);
        }

        protected internal virtual int CalculateRepeatValue(IJobEntity timerEntity)
        {
            int times = -1;
            IList<string> expression = new List<string>(timerEntity.Repeat.Split("/", true));
            if (expression.Count > 1 && expression[0].StartsWith("R", StringComparison.Ordinal) && expression[0].Length > 1)
            {
                times = int.Parse(expression[0].Substring(1));
                if (times > 0)
                {
                    times--;
                }
            }
            return times;
        }

        protected internal virtual string GetBusinessCalendarName(string calendarName, IVariableScope variableScope)
        {
            string businessCalendarName = CycleBusinessCalendar.NAME;
            if (!string.IsNullOrWhiteSpace(calendarName))
            {
                businessCalendarName = (string)Context.ProcessEngineConfiguration.ExpressionManager.CreateExpression(calendarName).GetValue(variableScope);
            }
            return businessCalendarName;
        }

        protected internal override IDataManager<ITimerJobEntity> DataManager
        {
            get
            {
                return jobDataManager;
            }
        }

        public virtual ITimerJobDataManager JobDataManager
        {
            set
            {
                this.jobDataManager = value;
            }
        }
    }

}