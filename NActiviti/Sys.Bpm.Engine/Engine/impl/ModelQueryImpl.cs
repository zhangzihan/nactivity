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

        private const long serialVersionUID = 1L;
        protected internal string id;
        protected internal string category;
        protected internal string categoryLike;
        protected internal string categoryNotEquals;
        protected internal string name;
        protected internal string nameLike;
        protected internal string key;
        protected internal int? version;
        protected internal bool latest;
        protected internal string deploymentId_Renamed;
        protected internal bool notDeployed_Renamed;
        protected internal bool deployed_Renamed;
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
            if (ReferenceEquals(category, null))
            {
                throw new ActivitiIllegalArgumentException("category is null");
            }
            this.category = category;
            return this;
        }

        public virtual IModelQuery modelCategoryLike(string categoryLike)
        {
            if (ReferenceEquals(categoryLike, null))
            {
                throw new ActivitiIllegalArgumentException("categoryLike is null");
            }
            this.categoryLike = categoryLike;
            return this;
        }

        public virtual IModelQuery modelCategoryNotEquals(string categoryNotEquals)
        {
            if (ReferenceEquals(categoryNotEquals, null))
            {
                throw new ActivitiIllegalArgumentException("categoryNotEquals is null");
            }
            this.categoryNotEquals = categoryNotEquals;
            return this;
        }

        public virtual IModelQuery modelName(string name)
        {
            if (ReferenceEquals(name, null))
            {
                throw new ActivitiIllegalArgumentException("name is null");
            }
            this.name = name;
            return this;
        }

        public virtual IModelQuery modelNameLike(string nameLike)
        {
            if (ReferenceEquals(nameLike, null))
            {
                throw new ActivitiIllegalArgumentException("nameLike is null");
            }
            this.nameLike = nameLike;
            return this;
        }

        public virtual IModelQuery modelKey(string key)
        {
            if (ReferenceEquals(key, null))
            {
                throw new ActivitiIllegalArgumentException("key is null");
            }
            this.key = key;
            return this;
        }

        public virtual IModelQuery modelVersion(int? version)
        {
            if (version == null)
            {
                throw new ActivitiIllegalArgumentException("version is null");
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
            if (ReferenceEquals(deploymentId, null))
            {
                throw new ActivitiIllegalArgumentException("DeploymentId is null");
            }
            this.deploymentId_Renamed = deploymentId;
            return this;
        }

        public virtual IModelQuery notDeployed()
        {
            if (deployed_Renamed)
            {
                throw new ActivitiIllegalArgumentException("Invalid usage: cannot use deployed() and notDeployed() in the same query");
            }
            this.notDeployed_Renamed = true;
            return this;
        }

        public virtual IModelQuery deployed()
        {
            if (notDeployed_Renamed)
            {
                throw new ActivitiIllegalArgumentException("Invalid usage: cannot use deployed() and notDeployed() in the same query");
            }
            this.deployed_Renamed = true;
            return this;
        }

        public virtual IModelQuery modelTenantId(string tenantId)
        {
            if (ReferenceEquals(tenantId, null))
            {
                throw new ActivitiIllegalArgumentException("Model tenant id is null");
            }
            this.tenantId = tenantId;
            return this;
        }

        public virtual IModelQuery modelTenantIdLike(string tenantIdLike)
        {
            if (ReferenceEquals(tenantIdLike, null))
            {
                throw new ActivitiIllegalArgumentException("Model tenant id is null");
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

        public virtual int? Version
        {
            get
            {
                return version;
            }
        }

        public virtual string Category
        {
            get
            {
                return category;
            }
        }

        public virtual string CategoryLike
        {
            get
            {
                return categoryLike;
            }
        }

        public virtual string CategoryNotEquals
        {
            get
            {
                return categoryNotEquals;
            }
        }

        public static long Serialversionuid
        {
            get
            {
                return serialVersionUID;
            }
        }

        public virtual string Key
        {
            get
            {
                return key;
            }
        }

        public virtual bool Latest
        {
            get
            {
                return latest;
            }
        }

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId_Renamed;
            }
        }

        public virtual bool NotDeployed
        {
            get
            {
                return notDeployed_Renamed;
            }
        }

        public virtual bool Deployed
        {
            get
            {
                return deployed_Renamed;
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

    }

}