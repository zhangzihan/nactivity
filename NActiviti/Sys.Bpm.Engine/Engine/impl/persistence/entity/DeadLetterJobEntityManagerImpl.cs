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

namespace Sys.Workflow.engine.impl.persistence.entity
{

    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.cfg;
    using Sys.Workflow.engine.impl.persistence.entity.data;
    using Sys.Workflow.engine.runtime;

    public class DeadLetterJobEntityManagerImpl : AbstractEntityManager<IDeadLetterJobEntity>, IDeadLetterJobEntityManager
    {

        protected internal IDeadLetterJobDataManager jobDataManager;

        public DeadLetterJobEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IDeadLetterJobDataManager jobDataManager) : base(processEngineConfiguration)
        {
            this.jobDataManager = jobDataManager;
        }

        public virtual IList<IDeadLetterJobEntity> FindJobsByExecutionId(string id)
        {
            return jobDataManager.FindJobsByExecutionId(id);
        }

        public virtual IList<IJob> FindJobsByQueryCriteria(DeadLetterJobQueryImpl jobQuery, Page page)
        {
            return jobDataManager.FindJobsByQueryCriteria(jobQuery, page);
        }

        public virtual long FindJobCountByQueryCriteria(DeadLetterJobQueryImpl jobQuery)
        {
            return jobDataManager.FindJobCountByQueryCriteria(jobQuery);
        }

        public virtual void UpdateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            jobDataManager.UpdateJobTenantIdForDeployment(deploymentId, newTenantId);
        }

        public override void Insert(IDeadLetterJobEntity jobEntity, bool fireCreateEvent)
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

                if (ExecutionRelatedEntityCountEnabledGlobally)
                {
                    ICountingExecutionEntity countingExecutionEntity = (ICountingExecutionEntity)execution;
                    if (IsExecutionRelatedEntityCountEnabled(countingExecutionEntity))
                    {
                        countingExecutionEntity.DeadLetterJobCount += 1;
                    }
                }
            }

            base.Insert(jobEntity, fireCreateEvent);
        }

        public override void Insert(IDeadLetterJobEntity jobEntity)
        {
            Insert(jobEntity, true);
        }

        public override void Delete(IDeadLetterJobEntity jobEntity)
        {
            base.Delete(jobEntity);

            DeleteExceptionByteArrayRef(jobEntity);

            if (!(jobEntity.ExecutionId is null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = ExecutionEntityManager.FindById<ICountingExecutionEntity>(jobEntity.ExecutionId);
                if (IsExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.DeadLetterJobCount -= 1;
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
        protected internal virtual void DeleteExceptionByteArrayRef(IDeadLetterJobEntity jobEntity)
        {
            if (jobEntity.ExceptionByteArrayRef is ByteArrayRef exceptionByteArrayRef)
            {
                exceptionByteArrayRef.Delete();
            }
        }

        protected internal virtual IDeadLetterJobEntity CreateDeadLetterJob(IAbstractJobEntity job)
        {
            IDeadLetterJobEntity newJobEntity = Create();
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