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

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.calendar;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.jobexecutor;
    using org.activiti.engine.impl.persistence.entity.data;
    using org.activiti.engine.runtime;
    using System.Linq;

    public class TimerJobEntityManagerImpl : AbstractEntityManager<ITimerJobEntity>, ITimerJobEntityManager
    {

        protected internal ITimerJobDataManager jobDataManager;

        public TimerJobEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, ITimerJobDataManager jobDataManager) : base(processEngineConfiguration)
        {
            this.jobDataManager = jobDataManager;
        }

        public virtual ITimerJobEntity createAndCalculateNextTimer(IJobEntity timerEntity, IVariableScope variableScope)
        {
            int repeatValue = calculateRepeatValue(timerEntity);
            if (repeatValue != 0)
            {
                if (repeatValue > 0)
                {
                    setNewRepeat(timerEntity, repeatValue);
                }
                DateTime? newTimer = calculateNextTimer(timerEntity, variableScope);
                if (newTimer != null && isValidTime(timerEntity, newTimer, variableScope))
                {
                    ITimerJobEntity te = createTimer(timerEntity);
                    te.Duedate = newTimer;
                    return te;
                }
            }
            return null;
        }

        public virtual IList<ITimerJobEntity> findTimerJobsToExecute(Page page)
        {
            return jobDataManager.findTimerJobsToExecute(page);
        }

        public virtual IList<ITimerJobEntity> findJobsByTypeAndProcessDefinitionId(string jobHandlerType, string processDefinitionId)
        {
            return jobDataManager.findJobsByTypeAndProcessDefinitionId(jobHandlerType, processDefinitionId);
        }

        public virtual IList<ITimerJobEntity> findJobsByTypeAndProcessDefinitionKeyNoTenantId(string jobHandlerType, string processDefinitionKey)
        {
            return jobDataManager.findJobsByTypeAndProcessDefinitionKeyNoTenantId(jobHandlerType, processDefinitionKey);
        }

        public virtual IList<ITimerJobEntity> findJobsByTypeAndProcessDefinitionKeyAndTenantId(string jobHandlerType, string processDefinitionKey, string tenantId)
        {
            return jobDataManager.findJobsByTypeAndProcessDefinitionKeyAndTenantId(jobHandlerType, processDefinitionKey, tenantId);
        }

        public virtual IList<ITimerJobEntity> findJobsByExecutionId(string id)
        {
            return jobDataManager.findJobsByExecutionId(id);
        }

        public virtual IList<ITimerJobEntity> findJobsByProcessInstanceId(string id)
        {
            return jobDataManager.findJobsByProcessInstanceId(id);
        }

        public virtual IList<IJob> findJobsByQueryCriteria(TimerJobQueryImpl jobQuery, Page page)
        {
            return jobDataManager.findJobsByQueryCriteria(jobQuery, page);
        }

        public virtual long findJobCountByQueryCriteria(TimerJobQueryImpl jobQuery)
        {
            return jobDataManager.findJobCountByQueryCriteria(jobQuery);
        }

        public virtual void updateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            jobDataManager.updateJobTenantIdForDeployment(deploymentId, newTenantId);
        }

        public virtual bool insertTimerJobEntity(ITimerJobEntity timerJobEntity)
        {
            return doInsert(timerJobEntity, true);
        }

        public override void insert(ITimerJobEntity jobEntity)
        {
            insert(jobEntity, true);
        }

        public override void insert(ITimerJobEntity jobEntity, bool fireCreateEvent)
        {
            doInsert(jobEntity, fireCreateEvent);
        }

        protected internal virtual bool doInsert(ITimerJobEntity jobEntity, bool fireCreateEvent)
        {
            // add link to execution
            if (!ReferenceEquals(jobEntity.ExecutionId, null))
            {
                IExecutionEntity execution = ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", jobEntity.ExecutionId));
                if (execution != null)
                {
                    execution.TimerJobs.Add(jobEntity);

                    // Inherit tenant if (if applicable)
                    if (!ReferenceEquals(execution.TenantId, null))
                    {
                        jobEntity.TenantId = execution.TenantId;
                    }

                    if (isExecutionRelatedEntityCountEnabled(execution))
                    {
                        ICountingExecutionEntity countingExecutionEntity = (ICountingExecutionEntity)execution;
                        countingExecutionEntity.TimerJobCount = countingExecutionEntity.TimerJobCount + 1;
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

            base.insert(jobEntity, fireCreateEvent);
            return true;
        }

        public override void delete(ITimerJobEntity jobEntity)
        {
            base.delete(jobEntity);

            deleteExceptionByteArrayRef(jobEntity);
            removeExecutionLink(jobEntity);

            if (!ReferenceEquals(jobEntity.ExecutionId, null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)ExecutionEntityManager.findById<ICountingExecutionEntity>(new KeyValuePair<string, object>("id", jobEntity.ExecutionId));
                if (isExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.TimerJobCount = executionEntity.TimerJobCount - 1;
                }
            }

            // Send event
            if (EventDispatcher.Enabled)
            {
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_DELETED, this));
            }
        }

        /// <summary>
        /// Removes the job's execution's reference to this job, if the job has an associated execution.
        /// Subclasses may override to provide custom implementations.
        /// </summary>
        protected internal virtual void removeExecutionLink(ITimerJobEntity jobEntity)
        {
            if (!ReferenceEquals(jobEntity.ExecutionId, null))
            {
                IExecutionEntity execution = ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", jobEntity.ExecutionId));
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
        protected internal virtual void deleteExceptionByteArrayRef(ITimerJobEntity jobEntity)
        {
            ByteArrayRef exceptionByteArrayRef = jobEntity.ExceptionByteArrayRef;
            if (exceptionByteArrayRef != null)
            {
                exceptionByteArrayRef.delete();
            }
        }

        protected internal virtual ITimerJobEntity createTimer(IJobEntity te)
        {
            ITimerJobEntity newTimerEntity = create();
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
            newTimerEntity.JobType = Job_Fields.JOB_TYPE_TIMER;
            return newTimerEntity;
        }

        protected internal virtual void setNewRepeat(IJobEntity timerEntity, int newRepeatValue)
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

        protected internal virtual bool isValidTime(IJobEntity timerEntity, DateTime? newTimerDate, IVariableScope variableScope)
        {
            IBusinessCalendar businessCalendar = ProcessEngineConfiguration.BusinessCalendarManager.getBusinessCalendar(getBusinessCalendarName(TimerEventHandler.geCalendarNameFromConfiguration(timerEntity.JobHandlerConfiguration), variableScope));
            return businessCalendar.validateDuedate(timerEntity.Repeat, timerEntity.MaxIterations, timerEntity.EndDate, newTimerDate).Value;
        }

        protected internal virtual DateTime? calculateNextTimer(IJobEntity timerEntity, IVariableScope variableScope)
        {
            IBusinessCalendar businessCalendar = ProcessEngineConfiguration.BusinessCalendarManager.getBusinessCalendar(getBusinessCalendarName(TimerEventHandler.geCalendarNameFromConfiguration(timerEntity.JobHandlerConfiguration), variableScope));
            return businessCalendar.resolveDuedate(timerEntity.Repeat, timerEntity.MaxIterations);
        }

        protected internal virtual int calculateRepeatValue(IJobEntity timerEntity)
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

        protected internal virtual string getBusinessCalendarName(string calendarName, IVariableScope variableScope)
        {
            string businessCalendarName = CycleBusinessCalendar.NAME;
            if (!string.IsNullOrWhiteSpace(calendarName))
            {
                businessCalendarName = (string)Context.ProcessEngineConfiguration.ExpressionManager.createExpression(calendarName).getValue(variableScope);
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