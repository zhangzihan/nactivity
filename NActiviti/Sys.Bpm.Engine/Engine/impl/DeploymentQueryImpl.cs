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
    using org.activiti.engine.impl.cmd;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.repository;

    /// 
    /// 
    [Serializable]
    public class DeploymentQueryImpl : AbstractQuery<IDeploymentQuery, IDeployment>, IDeploymentQuery
    {

        private const long serialVersionUID = 1L;
        protected internal string deploymentId_;
        protected internal string name;
        protected internal string nameLike;
        protected internal string category;
        protected internal string categoryLike;
        protected internal string categoryNotEquals;
        protected internal string key;
        protected internal string keyLike;
        protected internal string businessKey;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool withoutTenantId;
        protected internal string processDefinitionKey_;
        protected internal string processDefinitionKeyLike_;
        protected internal bool latest_Renamed;
        protected internal bool latestDeployment_;

        public DeploymentQueryImpl()
        {
        }

        public DeploymentQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public DeploymentQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IDeploymentQuery deploymentId(string deploymentId)
        {
            if (string.IsNullOrWhiteSpace(deploymentId))
            {
                this.deploymentId_ = null;
                return this;
            }
            this.deploymentId_ = deploymentId;
            return this;
        }

        public virtual IDeploymentQuery deploymentName(string deploymentName)
        {
            if (string.IsNullOrWhiteSpace(deploymentName))
            {
                this.name = null;
                return this;
            }
            this.name = deploymentName;
            return this;
        }

        public virtual IDeploymentQuery deploymentNameLike(string nameLike)
        {
            if (string.IsNullOrWhiteSpace(nameLike))
            {
                this.nameLike = null;
                return this;
            }
            this.nameLike = nameLike;
            return this;
        }

        public virtual IDeploymentQuery deploymentCategory(string deploymentCategory)
        {
            if (string.IsNullOrWhiteSpace(deploymentCategory))
            {
                this.category = null;
                return this;
            }
            this.category = deploymentCategory;
            return this;
        }

        public virtual IDeploymentQuery deploymentCategoryLike(string categoryLike)
        {
            if (string.IsNullOrWhiteSpace(categoryLike))
            {
                this.categoryLike = null;
                return this;
            }
            this.categoryLike = categoryLike;
            return this;
        }

        public virtual IDeploymentQuery deploymentCategoryNotEquals(string deploymentCategoryNotEquals)
        {
            if (string.IsNullOrWhiteSpace(deploymentCategoryNotEquals))
            {
                this.categoryNotEquals = null;
                return this;
            }
            this.categoryNotEquals = deploymentCategoryNotEquals;
            return this;
        }

        public virtual IDeploymentQuery deploymentKey(string deploymentKey)
        {
            if (string.IsNullOrWhiteSpace(deploymentKey))
            {
                this.key = null;
                return this;
            }
            this.key = deploymentKey;
            return this;
        }

        public virtual IDeploymentQuery deploymentKeyLike(string deploymentKeyLike)
        {
            if (string.IsNullOrWhiteSpace(deploymentKeyLike))
            {
                this.keyLike = null;
                return this;
            }
            this.keyLike = deploymentKeyLike;
            return this;
        }

        public virtual IDeploymentQuery deploymentBusinessKey(string businessKey)
        {
            if (string.IsNullOrWhiteSpace(businessKey))
            {
                this.businessKey = null;
                return this;
            }
            this.businessKey = businessKey;
            return this;
        }

        public virtual IDeploymentQuery deploymentTenantId(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                this.tenantId = null;
                return this;
            }
            this.tenantId = tenantId;
            return this;
        }

        public virtual IDeploymentQuery deploymentTenantIdLike(string tenantIdLike)
        {
            if (string.IsNullOrWhiteSpace(tenantIdLike))
            {
                this.tenantIdLike = null;
                return this;
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
            if (string.IsNullOrWhiteSpace(key))
            {
                this.processDefinitionKey_ = null;
                return this;
            }
            this.processDefinitionKey_ = key;
            return this;
        }

        public virtual IDeploymentQuery processDefinitionKeyLike(string keyLike)
        {
            if (string.IsNullOrWhiteSpace(keyLike))
            {
                this.processDefinitionKeyLike_ = null;
                return this;
            }
            this.processDefinitionKeyLike_ = keyLike;
            return this;
        }

        public virtual IDeploymentQuery latest()
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                this.latest_Renamed = false;
                return this;
            }
            //if (ReferenceEquals(key, null))
            //{
            //    throw new ActivitiIllegalArgumentException("latest can only be used together with a deployment key");
            //}

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

        public override long executeCount(ICommandContext commandContext)
        {
            checkQueryOk();
            return commandContext.DeploymentEntityManager.findDeploymentCountByQueryCriteria(this);
        }

        public override IList<IDeployment> executeList(ICommandContext commandContext, Page page)
        {
            checkQueryOk();
            return commandContext.DeploymentEntityManager.findDeploymentsByQueryCriteria(this, page);
        }

        public virtual IList<IDeployment> findDrafts()
        {
            return commandExecutor.execute<IList<IDeployment>>(new GetDraftDeploymentCmd(this));
        }

        // getters ////////////////////////////////////////////////////////

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId_;
            }
            set => deploymentId(value);
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
            set => deploymentName(value);
        }

        public virtual string NameLike
        {
            get
            {
                return nameLike;
            }
            set => deploymentNameLike(value);
        }

        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
            set => deploymentBusinessKey(value);
        }

        public virtual string Category
        {
            get
            {
                return category;
            }
            set => deploymentCategory(value);
        }

        public virtual string CategoryNotEquals
        {
            get
            {
                return categoryNotEquals;
            }
            set => deploymentCategoryNotEquals(value);
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set => deploymentTenantId(value);
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
            set => deploymentTenantIdLike(value);
        }

        public virtual bool WithoutTenantId
        {
            get
            {
                return withoutTenantId;
            }
            set => withoutTenantId = value;
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return processDefinitionKey_;
            }
            set => processDefinitionKey(value);
        }

        public virtual string ProcessDefinitionKeyLike
        {
            get
            {
                return processDefinitionKeyLike_;
            }
            set => processDefinitionKeyLike(value);
        }


        public virtual IDeploymentQuery latestDeployment()
        {
            latestDeployment_ = true;
            return this;
        }

        public bool LatestDeployment
        {
            get => latestDeployment_;
            set
            {
                if (value)
                {
                    latestDeployment();
                }
                else
                {
                    latestDeployment_ = value;
                }
            }
        }
    }
}