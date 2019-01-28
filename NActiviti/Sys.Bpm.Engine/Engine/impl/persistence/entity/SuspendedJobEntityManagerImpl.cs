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

    public class SuspendedJobEntityManagerImpl : AbstractEntityManager<ISuspendedJobEntity>, ISuspendedJobEntityManager
    {


        protected internal ISuspendedJobDataManager jobDataManager;

        public SuspendedJobEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, ISuspendedJobDataManager jobDataManager) : base(processEngineConfiguration)
        {
            this.jobDataManager = jobDataManager;
        }

        public virtual IList<ISuspendedJobEntity> findJobsByExecutionId(string id)
        {
            return jobDataManager.findJobsByExecutionId(id);
        }

        public virtual IList<ISuspendedJobEntity> findJobsByProcessInstanceId(string id)
        {
            return jobDataManager.findJobsByProcessInstanceId(id);
        }

        public virtual IList<IJob> findJobsByQueryCriteria(SuspendedJobQueryImpl jobQuery, Page page)
        {
            return jobDataManager.findJobsByQueryCriteria(jobQuery, page);
        }

        public virtual long findJobCountByQueryCriteria(SuspendedJobQueryImpl jobQuery)
        {
            return jobDataManager.findJobCountByQueryCriteria(jobQuery);
        }

        public virtual void updateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            jobDataManager.updateJobTenantIdForDeployment(deploymentId, newTenantId);
        }

        public override void insert(ISuspendedJobEntity jobEntity, bool fireCreateEvent)
        {

            // add link to execution
            if (!ReferenceEquals(jobEntity.ExecutionId, null))
            {
                IExecutionEntity execution = ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", jobEntity.ExecutionId));

                // Inherit tenant if (if applicable)
                if (!ReferenceEquals(execution.TenantId, null))
                {
                    jobEntity.TenantId = execution.TenantId;
                }

                if (isExecutionRelatedEntityCountEnabled(execution))
                {
                    ICountingExecutionEntity countingExecutionEntity = (ICountingExecutionEntity)execution;
                    countingExecutionEntity.SuspendedJobCount = countingExecutionEntity.SuspendedJobCount + 1;
                }
            }

            base.insert(jobEntity, fireCreateEvent);
        }

        public override void insert(ISuspendedJobEntity jobEntity)
        {
            insert(jobEntity, true);
        }

        public override void delete(ISuspendedJobEntity jobEntity)
        {
            base.delete(jobEntity);

            deleteExceptionByteArrayRef(jobEntity);

            if (!ReferenceEquals(jobEntity.ExecutionId, null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)ExecutionEntityManager.findById<ICountingExecutionEntity>(new KeyValuePair<string, object>("id", jobEntity.ExecutionId));
                if (isExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.SuspendedJobCount = executionEntity.SuspendedJobCount - 1;
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
        protected internal virtual void deleteExceptionByteArrayRef(ISuspendedJobEntity jobEntity)
        {
            ByteArrayRef exceptionByteArrayRef = jobEntity.ExceptionByteArrayRef;
            if (exceptionByteArrayRef != null)
            {
                exceptionByteArrayRef.delete();
            }
        }

        protected internal virtual ISuspendedJobEntity createSuspendedJob(IAbstractJobEntity job)
        {
            ISuspendedJobEntity newSuspendedJobEntity = create();
            newSuspendedJobEntity.JobHandlerConfiguration = job.JobHandlerConfiguration;
            newSuspendedJobEntity.JobHandlerType = job.JobHandlerType;
            newSuspendedJobEntity.Exclusive = job.Exclusive;
            newSuspendedJobEntity.Repeat = job.Repeat;
            newSuspendedJobEntity.Retries = job.Retries;
            newSuspendedJobEntity.EndDate = job.EndDate;
            newSuspendedJobEntity.ExecutionId = job.ExecutionId;
            newSuspendedJobEntity.ProcessInstanceId = job.ProcessInstanceId;
            newSuspendedJobEntity.ProcessDefinitionId = job.ProcessDefinitionId;

            // Inherit tenant
            newSuspendedJobEntity.TenantId = job.TenantId;
            newSuspendedJobEntity.JobType = job.JobType;
            return newSuspendedJobEntity;
        }

        protected internal override IDataManager<ISuspendedJobEntity> DataManager
        {
            get
            {
                return jobDataManager;
            }
        }

        public virtual ISuspendedJobDataManager JobDataManager
        {
            set
            {
                this.jobDataManager = value;
            }
        }
    }

}