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

namespace Sys.Workflow.engine.impl
{

    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.repository;

    /// 
    /// 
    [Serializable]
    public class ModelQueryImpl : AbstractQuery<IModelQuery, IModel>, IModelQuery
    {

        private static long serialVersionUID = 1L;
        protected internal string id;
        protected internal string category;
        protected internal string categoryLike;
        protected internal string categoryNotEquals;
        protected internal string name;
        protected internal string nameLike;
        protected internal string key;
        protected internal int? version;
        protected internal bool latest;
        protected internal string deploymentId_;
        protected internal bool notDeployed_;
        protected internal bool deployed_;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool withoutTenantId;

        public ModelQueryImpl()
        {
        }

        public ModelQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public ModelQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IModelQuery SetModelId(string modelId)
        {
            this.id = modelId;
            return this;
        }

        public virtual IModelQuery SetModelCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                this.category = null;
                return this;
            }
            this.category = category;
            return this;
        }

        public virtual IModelQuery SetModelCategoryLike(string categoryLike)
        {
            if (string.IsNullOrWhiteSpace(categoryLike))
            {
                this.categoryLike = null;
                return this;
            }
            this.categoryLike = categoryLike;
            return this;
        }

        public virtual IModelQuery SetModelCategoryNotEquals(string categoryNotEquals)
        {
            if (string.IsNullOrWhiteSpace(categoryNotEquals))
            {
                this.categoryNotEquals = null;
                return this;
            }
            this.categoryNotEquals = categoryNotEquals;
            return this;
        }

        public virtual IModelQuery SetModelName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                this.name = null;
                return this;
            }
            this.name = name;
            return this;
        }

        public virtual IModelQuery SetModelNameLike(string nameLike)
        {
            if (string.IsNullOrWhiteSpace(nameLike))
            {
                this.nameLike = null;
                return this;
            }
            this.nameLike = nameLike;
            return this;
        }

        public virtual IModelQuery SetModelKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                this.key = null;
                return this;
            }
            this.key = key;
            return this;
        }

        public virtual IModelQuery SetModelVersion(int? version)
        {
            if (version == null)
            {
                this.version = null;
            }
            else if (version <= 0)
            {
                throw new ActivitiIllegalArgumentException("version must be positive");
            }
            this.version = version;
            return this;
        }

        public virtual IModelQuery SetLatestVersion()
        {
            this.latest = true;
            return this;
        }

        public virtual IModelQuery SetDeploymentId(string deploymentId)
        {
            if (string.IsNullOrWhiteSpace(deploymentId))
            {
                this.deploymentId_ = null;
                return this;
            }
            this.deploymentId_ = deploymentId;
            return this;
        }

        public virtual IModelQuery SetNotDeployed()
        {
            if (deployed_)
            {
                throw new ActivitiIllegalArgumentException("Invalid usage: cannot use deployed() and notDeployed() in the same query");
            }
            this.notDeployed_ = true;
            return this;
        }

        public virtual IModelQuery SetDeployed()
        {
            if (notDeployed_)
            {
                throw new ActivitiIllegalArgumentException("Invalid usage: cannot use deployed() and notDeployed() in the same query");
            }
            this.deployed_ = true;
            return this;
        }

        public virtual IModelQuery SetModelTenantId(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                this.tenantId = null;
                return this;
            }
            this.tenantId = tenantId;
            return this;
        }

        public virtual IModelQuery SetModelTenantIdLike(string tenantIdLike)
        {
            if (string.IsNullOrWhiteSpace(tenantIdLike))
            {
                this.tenantIdLike = null;
                return this;
            }
            this.tenantIdLike = tenantIdLike;
            return this;
        }

        public virtual IModelQuery SetModelWithoutTenantId()
        {
            this.withoutTenantId = true;
            return this;
        }

        // sorting ////////////////////////////////////////////

        public virtual IModelQuery OrderByModelCategory()
        {
            return SetOrderBy(ModelQueryProperty.MODEL_CATEGORY);
        }

        public virtual IModelQuery OrderByModelId()
        {
            return SetOrderBy(ModelQueryProperty.MODEL_ID);
        }

        public virtual IModelQuery OrderByModelKey()
        {
            return SetOrderBy(ModelQueryProperty.MODEL_KEY);
        }

        public virtual IModelQuery OrderByModelVersion()
        {
            return SetOrderBy(ModelQueryProperty.MODEL_VERSION);
        }

        public virtual IModelQuery OrderByModelName()
        {
            return SetOrderBy(ModelQueryProperty.MODEL_NAME);
        }

        public virtual IModelQuery OrderByCreateTime()
        {
            return SetOrderBy(ModelQueryProperty.MODEL_CREATE_TIME);
        }

        public virtual IModelQuery OrderByLastUpdateTime()
        {
            return SetOrderBy(ModelQueryProperty.MODEL_LAST_UPDATE_TIME);
        }

        public virtual IModelQuery OrderByTenantId()
        {
            return SetOrderBy(ModelQueryProperty.MODEL_TENANT_ID);
        }

        // results ////////////////////////////////////////////

        public override long ExecuteCount(ICommandContext commandContext)
        {
            CheckQueryOk();
            return commandContext.ModelEntityManager.FindModelCountByQueryCriteria(this);
        }

        public override IList<IModel> ExecuteList(ICommandContext commandContext, Page page)
        {
            CheckQueryOk();
            return commandContext.ModelEntityManager.FindModelsByQueryCriteria(this, page);
        }

        // getters ////////////////////////////////////////////

        public virtual string Id
        {
            get
            {
                return id;
            }
            set => SetModelId(value);
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
            set => SetModelName(value);
        }

        public virtual string NameLike
        {
            get
            {
                return nameLike;
            }
            set => SetModelNameLike(value);
        }

        public virtual int? Version
        {
            get
            {
                return version;
            }
            set => SetModelVersion(value);
        }

        public virtual string Category
        {
            get
            {
                return category;
            }
            set => SetModelCategory(value);
        }

        public virtual string CategoryLike
        {
            get
            {
                return categoryLike;
            }
            set => SetModelCategoryLike(value);
        }

        public virtual string CategoryNotEquals
        {
            get
            {
                return categoryNotEquals;
            }
            set => SetModelCategoryNotEquals(value);
        }

        //public static long Serialversionuid
        //{
        //    get
        //    {
        //        return serialVersionUID;
        //    }
        //}

        public virtual string Key
        {
            get
            {
                return key;
            }
            set => SetModelKey(value);
        }

        public virtual bool Latest
        {
            get
            {
                return latest;
            }
            set => SetLatestVersion();
        }

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId_;
            }
            set => SetDeploymentId(value);
        }

        public virtual bool NotDeployed
        {
            get
            {
                return notDeployed_;
            }
            set => SetNotDeployed();
        }

        public virtual bool Deployed
        {
            get
            {
                return deployed_;
            }
            set => SetDeployed();
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set => SetModelTenantId(value);
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
            set => SetModelTenantIdLike(value);
        }

        public virtual bool WithoutTenantId
        {
            get
            {
                return withoutTenantId;
            }
            set => SetModelWithoutTenantId();
        }

    }

}