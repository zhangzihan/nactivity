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

    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.repository;

    [Serializable]
    public class ProcessDefinitionQueryImpl : AbstractQuery<IProcessDefinitionQuery, IProcessDefinition>, IProcessDefinitionQuery
    {

        private const long serialVersionUID = 1L;

        private string id;
        private ISet<string> ids;
        private string category;
        private string categoryLike;
        private string categoryNotEquals;
        private string name;
        private string nameLike;
        private string deploymentId_Renamed;
        private ISet<string> deploymentIds_Renamed;
        private string key;
        private string keyLike;
        private ISet<string> keys;
        private string resourceName;
        private string resourceNameLike;
        private int? version;
        private int? versionGt;
        private int? versionGte;
        private int? versionLt;
        private int? versionLte;
        private bool latest;
        private ISuspensionState suspensionState;
        private string authorizationUserId;
        private string procDefId;
        private string tenantId;
        private string tenantIdLike;
        private bool withoutTenantId;
        private string eventSubscriptionName;
        private string eventSubscriptionType;

        public ProcessDefinitionQueryImpl()
        {
        }

        public ProcessDefinitionQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public ProcessDefinitionQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IProcessDefinitionQuery processDefinitionId(string processDefinitionId)
        {
            this.id = processDefinitionId;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionIds(ISet<string> processDefinitionIds)
        {
            this.ids = processDefinitionIds;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionCategory(string category)
        {
            if (string.ReferenceEquals(category, null))
            {
                throw new ActivitiIllegalArgumentException("category is null");
            }
            this.category = category;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionCategoryLike(string categoryLike)
        {
            if (string.ReferenceEquals(categoryLike, null))
            {
                throw new ActivitiIllegalArgumentException("categoryLike is null");
            }
            this.categoryLike = categoryLike;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionCategoryNotEquals(string categoryNotEquals)
        {
            if (string.ReferenceEquals(categoryNotEquals, null))
            {
                throw new ActivitiIllegalArgumentException("categoryNotEquals is null");
            }
            this.categoryNotEquals = categoryNotEquals;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionName(string name)
        {
            if (string.ReferenceEquals(name, null))
            {
                throw new ActivitiIllegalArgumentException("name is null");
            }
            this.name = name;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionNameLike(string nameLike)
        {
            if (string.ReferenceEquals(nameLike, null))
            {
                throw new ActivitiIllegalArgumentException("nameLike is null");
            }
            this.nameLike = nameLike;
            return this;
        }

        public virtual IProcessDefinitionQuery deploymentId(string deploymentId)
        {
            if (string.ReferenceEquals(deploymentId, null))
            {
                throw new ActivitiIllegalArgumentException("id is null");
            }
            this.deploymentId_Renamed = deploymentId;
            return this;
        }

        public virtual IProcessDefinitionQuery deploymentIds(ISet<string> deploymentIds)
        {
            if (deploymentIds == null)
            {
                throw new ActivitiIllegalArgumentException("ids are null");
            }
            this.deploymentIds_Renamed = deploymentIds;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionKey(string key)
        {
            if (string.ReferenceEquals(key, null))
            {
                throw new ActivitiIllegalArgumentException("key is null");
            }
            this.key = key;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionKeys(ISet<string> keys)
        {
            if (keys == null)
            {
                throw new ActivitiIllegalArgumentException("keys is null");
            }
            this.keys = keys;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionKeyLike(string keyLike)
        {
            if (string.ReferenceEquals(keyLike, null))
            {
                throw new ActivitiIllegalArgumentException("keyLike is null");
            }
            this.keyLike = keyLike;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionResourceName(string resourceName)
        {
            if (string.ReferenceEquals(resourceName, null))
            {
                throw new ActivitiIllegalArgumentException("resourceName is null");
            }
            this.resourceName = resourceName;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionResourceNameLike(string resourceNameLike)
        {
            if (string.ReferenceEquals(resourceNameLike, null))
            {
                throw new ActivitiIllegalArgumentException("resourceNameLike is null");
            }
            this.resourceNameLike = resourceNameLike;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionVersion(int? version)
        {
            checkVersion(version);
            this.version = version;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionVersionGreaterThan(int? processDefinitionVersion)
        {
            checkVersion(processDefinitionVersion);
            this.versionGt = processDefinitionVersion;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionVersionGreaterThanOrEquals(int? processDefinitionVersion)
        {
            checkVersion(processDefinitionVersion);
            this.versionGte = processDefinitionVersion;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionVersionLowerThan(int? processDefinitionVersion)
        {
            checkVersion(processDefinitionVersion);
            this.versionLt = processDefinitionVersion;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionVersionLowerThanOrEquals(int? processDefinitionVersion)
        {
            checkVersion(processDefinitionVersion);
            this.versionLte = processDefinitionVersion;
            return this;
        }

        protected internal virtual void checkVersion(int? version)
        {
            if (version == null)
            {
                throw new ActivitiIllegalArgumentException("version is null");
            }
            else if (version <= 0)
            {
                throw new ActivitiIllegalArgumentException("version must be positive");
            }
        }

        public virtual IProcessDefinitionQuery latestVersion()
        {
            this.latest = true;
            return this;
        }

        public virtual IProcessDefinitionQuery active()
        {
            this.suspensionState = org.activiti.engine.impl.persistence.entity.SuspensionState_Fields.ACTIVE;
            return this;
        }

        public virtual IProcessDefinitionQuery suspended()
        {
            this.suspensionState = org.activiti.engine.impl.persistence.entity.SuspensionState_Fields.SUSPENDED;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionTenantId(string tenantId)
        {
            if (string.ReferenceEquals(tenantId, null))
            {
                throw new ActivitiIllegalArgumentException("processDefinition tenantId is null");
            }
            this.tenantId = tenantId;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionTenantIdLike(string tenantIdLike)
        {
            if (string.ReferenceEquals(tenantIdLike, null))
            {
                throw new ActivitiIllegalArgumentException("process definition tenantId is null");
            }
            this.tenantIdLike = tenantIdLike;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionWithoutTenantId()
        {
            this.withoutTenantId = true;
            return this;
        }

        public virtual IProcessDefinitionQuery messageEventSubscription(string messageName)
        {
            return eventSubscription("message", messageName);
        }

        public virtual IProcessDefinitionQuery messageEventSubscriptionName(string messageName)
        {
            return eventSubscription("message", messageName);
        }

        public virtual IProcessDefinitionQuery processDefinitionStarter(string procDefId)
        {
            this.procDefId = procDefId;
            return this;
        }

        public virtual IProcessDefinitionQuery eventSubscription(string eventType, string eventName)
        {
            if (string.ReferenceEquals(eventName, null))
            {
                throw new ActivitiIllegalArgumentException("event name is null");
            }
            if (string.ReferenceEquals(eventType, null))
            {
                throw new ActivitiException("event type is null");
            }
            this.eventSubscriptionType = eventType;
            this.eventSubscriptionName = eventName;
            return this;
        }

        public virtual IList<string> AuthorizationGroups
        {
            get
            {
                // Similar behaviour as the TaskQuery.taskCandidateUser() which
                // includes the groups the candidate
                // user is part of
                if (!string.ReferenceEquals(authorizationUserId, null))
                {
                    IUserGroupLookupProxy userGroupLookupProxy = Context.ProcessEngineConfiguration.UserGroupLookupProxy;
                    if (userGroupLookupProxy != null)
                    {
                        return userGroupLookupProxy.getGroupsForCandidateUser(authorizationUserId);
                    }
                    else
                    {
                        //log.warn("No UserGroupLookupProxy set on ProcessEngineConfiguration. Tasks queried only where user is directly related, not through groups.");
                    }
                }

                return null;
            }
        }

        // sorting ////////////////////////////////////////////

        public virtual IProcessDefinitionQuery orderByDeploymentId()
        {
            return orderBy(ProcessDefinitionQueryProperty.DEPLOYMENT_ID);
        }

        public virtual IProcessDefinitionQuery orderByProcessDefinitionKey()
        {
            return orderBy(ProcessDefinitionQueryProperty.PROCESS_DEFINITION_KEY);
        }

        public virtual IProcessDefinitionQuery orderByProcessDefinitionCategory()
        {
            return orderBy(ProcessDefinitionQueryProperty.PROCESS_DEFINITION_CATEGORY);
        }

        public virtual IProcessDefinitionQuery orderByProcessDefinitionId()
        {
            return orderBy(ProcessDefinitionQueryProperty.PROCESS_DEFINITION_ID);
        }

        public virtual IProcessDefinitionQuery orderByProcessDefinitionVersion()
        {
            return orderBy(ProcessDefinitionQueryProperty.PROCESS_DEFINITION_VERSION);
        }

        public virtual IProcessDefinitionQuery orderByProcessDefinitionName()
        {
            return orderBy(ProcessDefinitionQueryProperty.PROCESS_DEFINITION_NAME);
        }

        public virtual IProcessDefinitionQuery orderByTenantId()
        {
            return orderBy(ProcessDefinitionQueryProperty.PROCESS_DEFINITION_TENANT_ID);
        }

        // results ////////////////////////////////////////////

        public override long executeCount(ICommandContext commandContext)
        {
            checkQueryOk();

            return commandContext.ProcessDefinitionEntityManager.findProcessDefinitionCountByQueryCriteria(this);
        }

        public override IList<IProcessDefinition> executeList(ICommandContext commandContext, Page page)
        {
            checkQueryOk();

            return commandContext.ProcessDefinitionEntityManager.findProcessDefinitionsByQueryCriteria(this, page);
        }

        protected internal override void checkQueryOk()
        {
            base.checkQueryOk();
        }

        // getters ////////////////////////////////////////////

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId_Renamed;
            }
        }

        public virtual ISet<string> DeploymentIds
        {
            get
            {
                return deploymentIds_Renamed;
            }
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        public virtual ISet<string> Ids
        {
            get
            {
                return ids;
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

        public virtual string Key
        {
            get
            {
                return key;
            }
        }

        public virtual string KeyLike
        {
            get
            {
                return keyLike;
            }
        }

        public virtual ISet<string> Keys
        {
            get
            {
                return keys;
            }
        }

        public virtual int? Version
        {
            get
            {
                return version;
            }
        }

        public virtual int? VersionGt
        {
            get
            {
                return versionGt;
            }
        }

        public virtual int? VersionGte
        {
            get
            {
                return versionGte;
            }
        }

        public virtual int? VersionLt
        {
            get
            {
                return versionLt;
            }
        }

        public virtual int? VersionLte
        {
            get
            {
                return versionLte;
            }
        }

        public virtual bool Latest
        {
            get
            {
                return latest;
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

        public virtual string ResourceName
        {
            get
            {
                return resourceName;
            }
        }

        public virtual string ResourceNameLike
        {
            get
            {
                return resourceNameLike;
            }
        }

        public virtual ISuspensionState SuspensionState
        {
            get
            {
                return suspensionState;
            }
            set
            {
                this.suspensionState = value;
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

        public virtual string AuthorizationUserId
        {
            get
            {
                return authorizationUserId;
            }
        }

        public virtual string ProcDefId
        {
            get
            {
                return procDefId;
            }
        }

        public virtual string EventSubscriptionName
        {
            get
            {
                return eventSubscriptionName;
            }
        }

        public virtual string EventSubscriptionType
        {
            get
            {
                return eventSubscriptionType;
            }
        }

        public virtual IProcessDefinitionQuery startableByUser(string userId)
        {
            if (string.ReferenceEquals(userId, null))
            {
                throw new ActivitiIllegalArgumentException("userId is null");
            }
            this.authorizationUserId = userId;
            return this;
        }
    }

}