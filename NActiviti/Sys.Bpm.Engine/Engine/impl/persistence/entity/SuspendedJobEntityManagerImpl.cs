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

        public virtual IList<ISuspendedJobEntity> FindJobsByExecutionId(string id)
        {
            return jobDataManager.FindJobsByExecutionId(id);
        }

        public virtual IList<ISuspendedJobEntity> FindJobsByProcessInstanceId(string id)
        {
            return jobDataManager.FindJobsByProcessInstanceId(id);
        }

        public virtual IList<IJob> FindJobsByQueryCriteria(ISuspendedJobQuery jobQuery, Page page)
        {
            return jobDataManager.FindJobsByQueryCriteria(jobQuery, page);
        }

        public virtual long FindJobCountByQueryCriteria(ISuspendedJobQuery jobQuery)
        {
            return jobDataManager.FindJobCountByQueryCriteria(jobQuery);
        }

        public virtual void UpdateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            jobDataManager.UpdateJobTenantIdForDeployment(deploymentId, newTenantId);
        }

        public override void Insert(ISuspendedJobEntity jobEntity, bool fireCreateEvent)
        {

            // add link to execution
            if (!(jobEntity.ExecutionId is null))
            {
                IExecutionEntity execution = ExecutionEntityManager.FindById<IExecutionEntity>(jobEntity.ExecutionId);

                // Inherit tenant if (if applicable)
                if (!(execution.TenantId is null))
                {
                    jobEntity.TenantId = execution.TenantId;
                }

                if (IsExecutionRelatedEntityCountEnabled(execution))
                {
                    ICountingExecutionEntity countingExecutionEntity = (ICountingExecutionEntity)execution;
                    countingExecutionEntity.SuspendedJobCount += 1;
                }
            }

            base.Insert(jobEntity, fireCreateEvent);
        }

        public override void Insert(ISuspendedJobEntity jobEntity)
        {
            Insert(jobEntity, true);
        }

        public override void Delete(ISuspendedJobEntity jobEntity)
        {
            base.Delete(jobEntity);

            DeleteExceptionByteArrayRef(jobEntity);

            if (!(jobEntity.ExecutionId is null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = ExecutionEntityManager.FindById<ICountingExecutionEntity>(jobEntity.ExecutionId);
                if (IsExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.SuspendedJobCount -= 1;
                }
            }

            // Send event
            if (EventDispatcher.Enabled)
            {
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_DELETED, this));
            }
        }

        /// <summary>
        /// Deletes a the byte array used to store the exception information.  Subclasses may override
        /// to provide custom implementations.
        /// </summary>
        protected internal virtual void DeleteExceptionByteArrayRef(ISuspendedJobEntity jobEntity)
        {
            if (jobEntity.ExceptionByteArrayRef is ByteArrayRef exceptionByteArrayRef)
            {
                exceptionByteArrayRef.Delete();
            }
        }

        protected internal virtual ISuspendedJobEntity CreateSuspendedJob(IAbstractJobEntity job)
        {
            ISuspendedJobEntity newSuspendedJobEntity = Create();
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