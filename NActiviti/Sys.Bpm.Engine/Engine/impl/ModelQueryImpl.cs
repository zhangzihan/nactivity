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

        public virtual IModelQuery modelId(string modelId)
        {
            this.id = modelId;
            return this;
        }

        public virtual IModelQuery modelCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                this.category = null;
                return this;
            }
            this.category = category;
            return this;
        }

        public virtual IModelQuery modelCategoryLike(string categoryLike)
        {
            if (string.IsNullOrWhiteSpace(categoryLike))
            {
                this.categoryLike = null;
                return this;
            }
            this.categoryLike = categoryLike;
            return this;
        }

        public virtual IModelQuery modelCategoryNotEquals(string categoryNotEquals)
        {
            if (string.IsNullOrWhiteSpace(categoryNotEquals))
            {
                this.categoryNotEquals = null;
                return this;
            }
            this.categoryNotEquals = categoryNotEquals;
            return this;
        }

        public virtual IModelQuery modelName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                this.name = null;
                return this;
            }
            this.name = name;
            return this;
        }

        public virtual IModelQuery modelNameLike(string nameLike)
        {
            if (string.IsNullOrWhiteSpace(nameLike))
            {
                this.nameLike = null;
                return this;
            }
            this.nameLike = nameLike;
            return this;
        }

        public virtual IModelQuery modelKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                this.key = null;
                return this;
            }
            this.key = key;
            return this;
        }

        public virtual IModelQuery modelVersion(int? version)
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

        public virtual IModelQuery latestVersion()
        {
            this.latest = true;
            return this;
        }

        public virtual IModelQuery deploymentId(string deploymentId)
        {
            if (string.IsNullOrWhiteSpace(deploymentId))
            {
                this.deploymentId_ = null;
                return this;
            }
            this.deploymentId_ = deploymentId;
            return this;
        }

        public virtual IModelQuery notDeployed()
        {
            if (deployed_)
            {
                throw new ActivitiIllegalArgumentException("Invalid usage: cannot use deployed() and notDeployed() in the same query");
            }
            this.notDeployed_ = true;
            return this;
        }

        public virtual IModelQuery deployed()
        {
            if (notDeployed_)
            {
                throw new ActivitiIllegalArgumentException("Invalid usage: cannot use deployed() and notDeployed() in the same query");
            }
            this.deployed_ = true;
            return this;
        }

        public virtual IModelQuery modelTenantId(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                this.tenantId = null;
                return this;
            }
            this.tenantId = tenantId;
            return this;
        }

        public virtual IModelQuery modelTenantIdLike(string tenantIdLike)
        {
            if (string.IsNullOrWhiteSpace(tenantIdLike))
            {
                this.tenantIdLike = null;
                return this;
            }
            this.tenantIdLike = tenantIdLike;
            return this;
        }

        public virtual IModelQuery modelWithoutTenantId()
        {
            this.withoutTenantId = true;
            return this;
        }

        // sorting ////////////////////////////////////////////

        public virtual IModelQuery orderByModelCategory()
        {
            return orderBy(ModelQueryProperty.MODEL_CATEGORY);
        }

        public virtual IModelQuery orderByModelId()
        {
            return orderBy(ModelQueryProperty.MODEL_ID);
        }

        public virtual IModelQuery orderByModelKey()
        {
            return orderBy(ModelQueryProperty.MODEL_KEY);
        }

        public virtual IModelQuery orderByModelVersion()
        {
            return orderBy(ModelQueryProperty.MODEL_VERSION);
        }

        public virtual IModelQuery orderByModelName()
        {
            return orderBy(ModelQueryProperty.MODEL_NAME);
        }

        public virtual IModelQuery orderByCreateTime()
        {
            return orderBy(ModelQueryProperty.MODEL_CREATE_TIME);
        }

        public virtual IModelQuery orderByLastUpdateTime()
        {
            return orderBy(ModelQueryProperty.MODEL_LAST_UPDATE_TIME);
        }

        public virtual IModelQuery orderByTenantId()
        {
            return orderBy(ModelQueryProperty.MODEL_TENANT_ID);
        }

        // results ////////////////////////////////////////////

        public override long executeCount(ICommandContext commandContext)
        {
            checkQueryOk();
            return commandContext.ModelEntityManager.findModelCountByQueryCriteria(this);
        }

        public override IList<IModel> executeList(ICommandContext commandContext, Page page)
        {
            checkQueryOk();
            return commandContext.ModelEntityManager.findModelsByQueryCriteria(this, page);
        }

        // getters ////////////////////////////////////////////

        public virtual string Id
        {
            get
            {
                return id;
            }
            set => modelId(value);
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
            set => modelName(value);
        }

        public virtual string NameLike
        {
            get
            {
                return nameLike;
            }
            set => modelNameLike(value);
        }

        public virtual int? Version
        {
            get
            {
                return version;
            }
            set => modelVersion(value);
        }

        public virtual string Category
        {
            get
            {
                return category;
            }
            set => modelCategory(value);
        }

        public virtual string CategoryLike
        {
            get
            {
                return categoryLike;
            }
            set => modelCategoryLike(value);
        }

        public virtual string CategoryNotEquals
        {
            get
            {
                return categoryNotEquals;
            }
            set => modelCategoryNotEquals(value);
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
            set => modelKey(value);
        }

        public virtual bool Latest
        {
            get
            {
                return latest;
            }
            set => latestVersion();
        }

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId_;
            }
            set => deploymentId(value);
        }

        public virtual bool NotDeployed
        {
            get
            {
                return notDeployed_;
            }
            set => notDeployed();
        }

        public virtual bool Deployed
        {
            get
            {
                return deployed_;
            }
            set => deployed();
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set => modelTenantId(value);
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
            set => modelTenantIdLike(value);
        }

        public virtual bool WithoutTenantId
        {
            get
            {
                return withoutTenantId;
            }
            set => modelWithoutTenantId();
        }

    }

}