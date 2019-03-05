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

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data;
    using org.activiti.engine.runtime;

    public class JobEntityManagerImpl : AbstractEntityManager<IJobEntity>, IJobEntityManager
    {

        protected internal IJobDataManager jobDataManager;

        public JobEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IJobDataManager jobDataManager) : base(processEngineConfiguration)
        {
            this.jobDataManager = jobDataManager;
        }

        protected internal override IDataManager<IJobEntity> DataManager
        {
            get
            {
                return jobDataManager;
            }
        }

        public virtual bool insertJobEntity(IJobEntity timerJobEntity)
        {
            return doInsert(timerJobEntity, true);
        }

        public override void insert(IJobEntity jobEntity, bool fireCreateEvent)
        {
            doInsert(jobEntity, fireCreateEvent);
        }

        protected internal virtual bool doInsert(IJobEntity jobEntity, bool fireCreateEvent)
        {
            // add link to execution
            if (!ReferenceEquals(jobEntity.ExecutionId, null))
            {
                IExecutionEntity execution = ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", jobEntity.ExecutionId));
                if (execution != null)
                {
                    execution.Jobs.Add(jobEntity);

                    // Inherit tenant if (if applicable)
                    if (!ReferenceEquals(execution.TenantId, null))
                    {
                        jobEntity.TenantId = execution.TenantId;
                    }

                    if (isExecutionRelatedEntityCountEnabled(execution))
                    {
                        ICountingExecutionEntity countingExecutionEntity = (ICountingExecutionEntity)execution;
                        countingExecutionEntity.JobCount = countingExecutionEntity.JobCount + 1;
                    }
                }
                else
                {
                    return false;
                }
            }

            base.insert(jobEntity, fireCreateEvent);
            return true;
        }

        public virtual IList<IJobEntity> findJobsToExecute(Page page)
        {
            return jobDataManager.findJobsToExecute(page);
        }

        public virtual IList<IJobEntity> findJobsByExecutionId(string executionId)
        {
            return jobDataManager.findJobsByExecutionId(executionId);
        }

        public virtual IList<IJobEntity> findJobsByProcessDefinitionId(string processDefinitionId)
        {
            return jobDataManager.findJobsByProcessDefinitionId(processDefinitionId);
        }

        public virtual IList<IJobEntity> findJobsByTypeAndProcessDefinitionId(string jobTypeTimer, string id)
        {
            return jobDataManager.findJobsByTypeAndProcessDefinitionId(jobTypeTimer, id);
        }

        public virtual IList<IJobEntity> findJobsByProcessInstanceId(string processInstanceId)
        {
            return jobDataManager.findJobsByProcessInstanceId(processInstanceId);
        }

        public virtual IList<IJobEntity> findExpiredJobs(Page page)
        {
            return jobDataManager.findExpiredJobs(page);
        }

        public virtual void resetExpiredJob(string jobId)
        {
            jobDataManager.resetExpiredJob(jobId);
        }

        public virtual IList<IJob> findJobsByQueryCriteria(JobQueryImpl jobQuery, Page page)
        {
            return jobDataManager.findJobsByQueryCriteria(jobQuery, page);
        }

        public virtual long findJobCountByQueryCriteria(JobQueryImpl jobQuery)
        {
            return jobDataManager.findJobCountByQueryCriteria(jobQuery);
        }

        public virtual void updateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            jobDataManager.updateJobTenantIdForDeployment(deploymentId, newTenantId);
        }

        public override void delete(IJobEntity jobEntity)
        {
            base.delete(jobEntity);

            deleteExceptionByteArrayRef(jobEntity);

            removeExecutionLink(jobEntity);

            // Send event
            if (EventDispatcher.Enabled)
            {
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_DELETED, this));
            }
        }

        public override void delete(IJobEntity entity, bool fireDeleteEvent)
        {
            if (!ReferenceEquals(entity.ExecutionId, null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)ExecutionEntityManager.findById<ICountingExecutionEntity>(new KeyValuePair<string, object>("id", entity.ExecutionId));
                if (isExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.JobCount = executionEntity.JobCount - 1;
                }
            }
            base.delete(entity, fireDeleteEvent);
        }

        /// <summary>
        /// Removes the job's execution's reference to this job, if the job has an associated execution.
        /// Subclasses may override to provide custom implementations.
        /// </summary>
        protected internal virtual void removeExecutionLink(IJobEntity jobEntity)
        {
            if (!ReferenceEquals(jobEntity.ExecutionId, null))
            {
                IExecutionEntity execution = ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", jobEntity.ExecutionId));
                if (execution != null)
                {
                    execution.Jobs.Remove(jobEntity);
                }
            }
        }

        /// <summary>
        /// Deletes a the byte array used to store the exception information.  Subclasses may override
        /// to provide custom implementations.
        /// </summary>
        protected internal virtual void deleteExceptionByteArrayRef(IJobEntity jobEntity)
        {
            ByteArrayRef exceptionByteArrayRef = jobEntity.ExceptionByteArrayRef as ByteArrayRef;
            if (exceptionByteArrayRef != null)
            {
                exceptionByteArrayRef.delete();
            }
        }

        public virtual IJobDataManager JobDataManager
        {
            get
            {
                return jobDataManager;
            }
            set
            {
                this.jobDataManager = value;
            }
        }

    }

}