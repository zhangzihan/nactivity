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

    public class DeadLetterJobEntityManagerImpl : AbstractEntityManager<IDeadLetterJobEntity>, IDeadLetterJobEntityManager
    {

        protected internal IDeadLetterJobDataManager jobDataManager;

        public DeadLetterJobEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IDeadLetterJobDataManager jobDataManager) : base(processEngineConfiguration)
        {
            this.jobDataManager = jobDataManager;
        }

        public virtual IList<IDeadLetterJobEntity> findJobsByExecutionId(string id)
        {
            return jobDataManager.findJobsByExecutionId(id);
        }

        public virtual IList<IJob> findJobsByQueryCriteria(DeadLetterJobQueryImpl jobQuery, Page page)
        {
            return jobDataManager.findJobsByQueryCriteria(jobQuery, page);
        }

        public virtual long findJobCountByQueryCriteria(DeadLetterJobQueryImpl jobQuery)
        {
            return jobDataManager.findJobCountByQueryCriteria(jobQuery);
        }

        public virtual void updateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            jobDataManager.updateJobTenantIdForDeployment(deploymentId, newTenantId);
        }

        public override void insert(IDeadLetterJobEntity jobEntity, bool fireCreateEvent)
        {

            // add link to execution
            if (!ReferenceEquals(jobEntity.ExecutionId, null))
            {
                IExecutionEntity execution = ExecutionEntityManager.findById<IExecutionEntity>(jobEntity.ExecutionId);

                // Inherit tenant if (if applicable)
                if (!ReferenceEquals(execution.TenantId, null))
                {
                    jobEntity.TenantId = execution.TenantId;
                }

                if (ExecutionRelatedEntityCountEnabledGlobally)
                {
                    ICountingExecutionEntity countingExecutionEntity = (ICountingExecutionEntity)execution;
                    if (isExecutionRelatedEntityCountEnabled(countingExecutionEntity))
                    {
                        countingExecutionEntity.DeadLetterJobCount = countingExecutionEntity.DeadLetterJobCount + 1;
                    }
                }
            }

            base.insert(jobEntity, fireCreateEvent);
        }

        public override void insert(IDeadLetterJobEntity jobEntity)
        {
            insert(jobEntity, true);
        }

        public override void delete(IDeadLetterJobEntity jobEntity)
        {
            base.delete(jobEntity);

            deleteExceptionByteArrayRef(jobEntity);

            if (!ReferenceEquals(jobEntity.ExecutionId, null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)ExecutionEntityManager.findById<ICountingExecutionEntity>(jobEntity.ExecutionId);
                if (isExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.DeadLetterJobCount = executionEntity.DeadLetterJobCount - 1;
                }
            }

            // Send event
            if (EventDispatcher.Enabled)
            {
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_DELETED, this));
            }
        }

        /// <summary>
        /// Deletes a the byte array used to store the exception information.  Subclasses may override
        /// to provide custom implementations.
        /// </summary>
        protected internal virtual void deleteExceptionByteArrayRef(IDeadLetterJobEntity jobEntity)
        {
            ByteArrayRef exceptionByteArrayRef = jobEntity.ExceptionByteArrayRef as ByteArrayRef;
            if (exceptionByteArrayRef != null)
            {
                exceptionByteArrayRef.delete();
            }
        }

        protected internal virtual IDeadLetterJobEntity createDeadLetterJob(IAbstractJobEntity job)
        {
            IDeadLetterJobEntity newJobEntity = create();
            newJobEntity.JobHandlerConfiguration = job.JobHandlerConfiguration;
            newJobEntity.JobHandlerType = job.JobHandlerType;
            newJobEntity.Exclusive = job.Exclusive;
            newJobEntity.Repeat = job.Repeat;
            newJobEntity.Retries = job.Retries;
            newJobEntity.EndDate = job.EndDate;
            newJobEntity.ExecutionId = job.ExecutionId;
            newJobEntity.ProcessInstanceId = job.ProcessInstanceId;
            newJobEntity.ProcessDefinitionId = job.ProcessDefinitionId;

            // Inherit tenant
            newJobEntity.TenantId = job.TenantId;
            newJobEntity.JobType = job.JobType;
            return newJobEntity;
        }

        protected internal override IDataManager<IDeadLetterJobEntity> DataManager
        {
            get
            {
                return jobDataManager;
            }
        }

        public virtual IDeadLetterJobDataManager JobDataManager
        {
            set
            {
                this.jobDataManager = value;
            }
        }
    }

}