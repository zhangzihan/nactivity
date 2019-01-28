using System;
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

namespace org.activiti.engine.impl
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.repository;

    /// 
    /// 
    [Serializable]
    public class DeploymentQueryImpl : AbstractQuery<IDeploymentQuery, IDeployment>, IDeploymentQuery
    {

        private const long serialVersionUID = 1L;
        protected internal string deploymentId_Renamed;
        protected internal string name;
        protected internal string nameLike;
        protected internal string category;
        protected internal string categoryLike;
        protected internal string categoryNotEquals;
        protected internal string key;
        protected internal string keyLike;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool withoutTenantId;
        protected internal string processDefinitionKey_Renamed;
        protected internal string processDefinitionKeyLike_Renamed;
        protected internal bool latest_Renamed;

        public DeploymentQueryImpl()
        {
        }

        public  DeploymentQueryImpl(ICommandContext  commandContext) : base(commandContext)
        {
        }

        public DeploymentQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IDeploymentQuery deploymentId(string deploymentId)
        {
            if (ReferenceEquals(deploymentId, null))
            {
                throw new ActivitiIllegalArgumentException("Deployment id is null");
            }
            this.deploymentId_Renamed = deploymentId;
            return this;
        }

        public virtual IDeploymentQuery deploymentName(string deploymentName)
        {
            if (ReferenceEquals(deploymentName, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentName is null");
            }
            this.name = deploymentName;
            return this;
        }

        public virtual IDeploymentQuery deploymentNameLike(string nameLike)
        {
            if (ReferenceEquals(nameLike, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentNameLike is null");
            }
            this.nameLike = nameLike;
            return this;
        }

        public virtual IDeploymentQuery deploymentCategory(string deploymentCategory)
        {
            if (ReferenceEquals(deploymentCategory, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentCategory is null");
            }
            this.category = deploymentCategory;
            return this;
        }

        public virtual IDeploymentQuery deploymentCategoryLike(string categoryLike)
        {
            if (ReferenceEquals(categoryLike, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentCategoryLike is null");
            }
            this.categoryLike = categoryLike;
            return this;
        }

        public virtual IDeploymentQuery deploymentCategoryNotEquals(string deploymentCategoryNotEquals)
        {
            if (ReferenceEquals(deploymentCategoryNotEquals, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentCategoryExclude is null");
            }
            this.categoryNotEquals = deploymentCategoryNotEquals;
            return this;
        }

        public virtual IDeploymentQuery deploymentKey(string deploymentKey)
        {
            if (ReferenceEquals(deploymentKey, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentKey is null");
            }
            this.key = deploymentKey;
            return this;
        }

        public virtual IDeploymentQuery deploymentKeyLike(string deploymentKeyLike)
        {
            if (ReferenceEquals(deploymentKeyLike, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentKeyLike is null");
            }
            this.keyLike = deploymentKeyLike;
            return this;
        }

        public virtual IDeploymentQuery deploymentTenantId(string tenantId)
        {
            if (ReferenceEquals(tenantId, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentTenantId is null");
            }
            this.tenantId = tenantId;
            return this;
        }

        public virtual IDeploymentQuery deploymentTenantIdLike(string tenantIdLike)
        {
            if (ReferenceEquals(tenantIdLike, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentTenantIdLike is null");
            }
            this.tenantIdLike = tenantIdLike;
            return this;
        }

        public virtual IDeploymentQuery deploymentWithoutTenantId()
        {
            this.withoutTenantId = true;
            return this;
        }

        public virtual IDeploymentQuery processDefinitionKey(string key)
        {
            if (ReferenceEquals(key, null))
            {
                throw new ActivitiIllegalArgumentException("key is null");
            }
            this.processDefinitionKey_Renamed = key;
            return this;
        }

        public virtual IDeploymentQuery processDefinitionKeyLike(string keyLike)
        {
            if (ReferenceEquals(keyLike, null))
            {
                throw new ActivitiIllegalArgumentException("keyLike is null");
            }
            this.processDefinitionKeyLike_Renamed = keyLike;
            return this;
        }

        public virtual IDeploymentQuery latest()
        {
            if (ReferenceEquals(key, null))
            {
                throw new ActivitiIllegalArgumentException("latest can only be used together with a deployment key");
            }

            this.latest_Renamed = true;
            return this;
        }

        // sorting ////////////////////////////////////////////////////////

        public virtual IDeploymentQuery orderByDeploymentId()
        {
            return orderBy(DeploymentQueryProperty.DEPLOYMENT_ID);
        }

        public virtual IDeploymentQuery orderByDeploymenTime()
        {
            return orderBy(DeploymentQueryProperty.DEPLOY_TIME);
        }

        public virtual IDeploymentQuery orderByDeploymentName()
        {
            return orderBy(DeploymentQueryProperty.DEPLOYMENT_NAME);
        }

        public virtual IDeploymentQuery orderByTenantId()
        {
            return orderBy(DeploymentQueryProperty.DEPLOYMENT_TENANT_ID);
        }

        // results ////////////////////////////////////////////////////////

        public  override long executeCount(ICommandContext  commandContext)
        {
            checkQueryOk();
            return commandContext.DeploymentEntityManager.findDeploymentCountByQueryCriteria(this);
        }

        public  override IList<IDeployment> executeList(ICommandContext  commandContext, Page page)
        {
            checkQueryOk();
            return commandContext.DeploymentEntityManager.findDeploymentsByQueryCriteria(this, page);
        }

        // getters ////////////////////////////////////////////////////////

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId_Renamed;
            }
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
        }

        public virtual string NameLike
        {
            get
            {
                return nameLike;
            }
        }

        public virtual string Category
        {
            get
            {
                return category;
            }
        }

        public virtual string CategoryNotEquals
        {
            get
            {
                return categoryNotEquals;
            }
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
        }

        public virtual bool WithoutTenantId
        {
            get
            {
                return withoutTenantId;
            }
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return processDefinitionKey_Renamed;
            }
        }

        public virtual string ProcessDefinitionKeyLike
        {
            get
            {
                return processDefinitionKeyLike_Renamed;
            }
        }
    }

}