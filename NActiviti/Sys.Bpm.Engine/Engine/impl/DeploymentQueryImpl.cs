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

namespace Sys.Workflow.Engine.Impl
{
    using Sys.Workflow.Engine.Impl.Cmd;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Repository;
    using System.Linq;

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
        protected internal bool latest_;
        protected internal bool latestDeployment_;
        protected internal bool onlyDrafts_;

        public DeploymentQueryImpl()
        {
        }

        public DeploymentQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public DeploymentQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IDeploymentQuery SetDeploymentId(string deploymentId)
        {
            if (string.IsNullOrWhiteSpace(deploymentId))
            {
                this.deploymentId_ = null;
                return this;
            }
            this.deploymentId_ = deploymentId;
            return this;
        }

        /// <inheritdoc />
        public virtual IDeploymentQuery SetDeploymentIds(string[] ids)
        {
            if ((ids?.Length).GetValueOrDefault(0) == 0 || ids.Any(x => string.IsNullOrWhiteSpace(x) == false))
            {
                this.Ids = null;
                return this;
            }
            this.Ids = ids;
            return this;
        }

        public virtual IDeploymentQuery SetDeploymentName(string deploymentName)
        {
            if (string.IsNullOrWhiteSpace(deploymentName))
            {
                this.name = null;
                return this;
            }
            this.name = deploymentName;
            return this;
        }

        public virtual IDeploymentQuery SetDeploymentNameLike(string nameLike)
        {
            if (string.IsNullOrWhiteSpace(nameLike))
            {
                this.nameLike = null;
                return this;
            }
            this.nameLike = nameLike;
            return this;
        }

        public virtual IDeploymentQuery SetDeploymentCategory(string deploymentCategory)
        {
            if (string.IsNullOrWhiteSpace(deploymentCategory))
            {
                this.category = null;
                return this;
            }
            this.category = deploymentCategory;
            return this;
        }

        public virtual IDeploymentQuery SetDeploymentCategoryLike(string categoryLike)
        {
            if (string.IsNullOrWhiteSpace(categoryLike))
            {
                this.categoryLike = null;
                return this;
            }
            this.categoryLike = categoryLike;
            return this;
        }

        public virtual IDeploymentQuery SetDeploymentCategoryNotEquals(string deploymentCategoryNotEquals)
        {
            if (string.IsNullOrWhiteSpace(deploymentCategoryNotEquals))
            {
                this.categoryNotEquals = null;
                return this;
            }
            this.categoryNotEquals = deploymentCategoryNotEquals;
            return this;
        }

        public virtual IDeploymentQuery SetDeploymentKey(string deploymentKey)
        {
            if (string.IsNullOrWhiteSpace(deploymentKey))
            {
                this.key = null;
                return this;
            }
            this.key = deploymentKey;
            return this;
        }

        public virtual IDeploymentQuery SetDeploymentKeyLike(string deploymentKeyLike)
        {
            if (string.IsNullOrWhiteSpace(deploymentKeyLike))
            {
                this.keyLike = null;
                return this;
            }
            this.keyLike = deploymentKeyLike;
            return this;
        }

        public virtual IDeploymentQuery SetDeploymentBusinessKey(string businessKey)
        {
            if (string.IsNullOrWhiteSpace(businessKey))
            {
                this.businessKey = null;
                return this;
            }
            this.businessKey = businessKey;
            return this;
        }

        public virtual IDeploymentQuery SetDeploymentTenantId(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                this.tenantId = null;
                return this;
            }
            this.tenantId = tenantId;
            return this;
        }

        public virtual IDeploymentQuery SetDeploymentTenantIdLike(string tenantIdLike)
        {
            if (string.IsNullOrWhiteSpace(tenantIdLike))
            {
                this.tenantIdLike = null;
                return this;
            }
            this.tenantIdLike = tenantIdLike;
            return this;
        }

        public virtual IDeploymentQuery SetDeploymentWithoutTenantId()
        {
            this.withoutTenantId = true;
            return this;
        }

        public virtual IDeploymentQuery SetProcessDefinitionKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                this.processDefinitionKey_ = null;
                return this;
            }
            this.processDefinitionKey_ = key;
            return this;
        }

        public virtual IDeploymentQuery SetProcessDefinitionKeyLike(string keyLike)
        {
            if (string.IsNullOrWhiteSpace(keyLike))
            {
                this.processDefinitionKeyLike_ = null;
                return this;
            }
            this.processDefinitionKeyLike_ = keyLike;
            return this;
        }

        public virtual IDeploymentQuery SetLatest()
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                this.latest_ = false;
                return this;
            }
            //if (ReferenceEquals(key, null))
            //{
            //    throw new ActivitiIllegalArgumentException("latest can only be used together with a deployment key");
            //}

            this.latest_ = true;
            return this;
        }

        public virtual IDeploymentQuery SetOnlyDrafts()
        {
            this.onlyDrafts_ = true;
            return this;
        }

        // sorting ////////////////////////////////////////////////////////

        public virtual IDeploymentQuery SetOrderByDeploymentId()
        {
            return SetOrderBy(DeploymentQueryProperty.DEPLOYMENT_ID);
        }

        public virtual IDeploymentQuery SetOrderByDeploymenTime()
        {
            return SetOrderBy(DeploymentQueryProperty.DEPLOY_TIME);
        }

        public virtual IDeploymentQuery SetOrderByDeploymentName()
        {
            return SetOrderBy(DeploymentQueryProperty.DEPLOYMENT_NAME);
        }

        public virtual IDeploymentQuery SetOrderByTenantId()
        {
            return SetOrderBy(DeploymentQueryProperty.DEPLOYMENT_TENANT_ID);
        }

        // results ////////////////////////////////////////////////////////

        public override long ExecuteCount(ICommandContext commandContext)
        {
            CheckQueryOk();
            return commandContext.DeploymentEntityManager.FindDeploymentCountByQueryCriteria(this);
        }

        public override IList<IDeployment> ExecuteList(ICommandContext commandContext, Page page)
        {
            CheckQueryOk();
            return commandContext.DeploymentEntityManager.FindDeploymentsByQueryCriteria(this, page);
        }

        public virtual IList<IDeployment> FindDrafts()
        {
            return commandExecutor.Execute<IList<IDeployment>>(new GetDraftDeploymentCmd(this));
        }

        // getters ////////////////////////////////////////////////////////

        public virtual string[] Ids
        {
            get; set;
        }

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId_;
            }
            set => SetDeploymentId(value);
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
            set => SetDeploymentName(value);
        }

        public virtual string NameLike
        {
            get
            {
                return nameLike;
            }
            set => SetDeploymentNameLike(value);
        }

        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
            set => SetDeploymentBusinessKey(value);
        }

        public virtual string Category
        {
            get
            {
                return category;
            }
            set => SetDeploymentCategory(value);
        }

        public virtual string CategoryNotEquals
        {
            get
            {
                return categoryNotEquals;
            }
            set => SetDeploymentCategoryNotEquals(value);
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set => SetDeploymentTenantId(value);
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
            set => SetDeploymentTenantIdLike(value);
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
            set => SetProcessDefinitionKey(value);
        }

        public virtual string ProcessDefinitionKeyLike
        {
            get
            {
                return processDefinitionKeyLike_;
            }
            set => SetProcessDefinitionKeyLike(value);
        }


        public virtual IDeploymentQuery SetLatestDeployment()
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
                    SetLatestDeployment();
                }
                else
                {
                    latestDeployment_ = value;
                }
            }
        }

        public bool OnlyDrafts
        {
            get => onlyDrafts_;
            set
            {
                if (value)
                {
                    SetOnlyDrafts();
                }
                else
                {
                    onlyDrafts_ = false;
                }
            }
        }
    }
}