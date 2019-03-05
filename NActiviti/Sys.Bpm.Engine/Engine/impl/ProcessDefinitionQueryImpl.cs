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
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.repository;
    using Sys;

    [Serializable]
    public class ProcessDefinitionQueryImpl : AbstractQuery<IProcessDefinitionQuery, IProcessDefinition>, IProcessDefinitionQuery
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<ProcessDefinitionQueryImpl>();

        private const long serialVersionUID = 1L;

        private string id;
        private ISet<string> ids;
        private string category;
        private string categoryLike;
        private string categoryNotEquals;
        private string businessKey;
        private string businessPath;
        private string startForm;
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
            //这里没看明白activiti为什么抛出异常,允许从业务角度讲可以理解,但是从程序角度讲有点限制死了.
            //if (ReferenceEquals(category, null))
            //{
            //    throw new ActivitiIllegalArgumentException("category is null");
            //}
            if (string.IsNullOrWhiteSpace(category))
            {
                this.category = null;
                return this;
            }
            this.category = category;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionCategoryLike(string categoryLike)
        {
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(categoryLike, null))
            //{
            //    throw new ActivitiIllegalArgumentException("categoryLike is null");
            //}
            if (string.IsNullOrWhiteSpace(categoryLike))
            {
                this.categoryLike = null;
                return this;
            }
            this.categoryLike = categoryLike;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionCategoryNotEquals(string categoryNotEquals)
        {
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(categoryNotEquals, null))
            //{
            //    throw new ActivitiIllegalArgumentException("categoryNotEquals is null");
            //}
            if (string.IsNullOrWhiteSpace(categoryNotEquals))
            {
                this.categoryNotEquals = null;
                return this;
            }
            this.categoryNotEquals = categoryNotEquals;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionName(string name)
        {
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(name, null))
            //{
            //    throw new ActivitiIllegalArgumentException("name is null");
            //}
            if (string.IsNullOrWhiteSpace(name))
            {
                this.name = null;
                return this;
            }
            this.name = name;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionNameLike(string nameLike)
        {
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(nameLike, null))
            //{
            //    throw new ActivitiIllegalArgumentException("nameLike is null");
            //}
            if (string.IsNullOrWhiteSpace(nameLike))
            {
                this.nameLike = null;
                return this;
            }
            this.nameLike = nameLike;
            return this;
        }

        public virtual IProcessDefinitionQuery deploymentId(string deploymentId)
        {
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(deploymentId, null))
            //{
            //    throw new ActivitiIllegalArgumentException("id is null");
            //}
            if (string.IsNullOrWhiteSpace(deploymentId))
            {
                this.deploymentId_Renamed = null;
                return this;
            }
            this.deploymentId_Renamed = deploymentId;
            return this;
        }

        public virtual IProcessDefinitionQuery deploymentIds(ISet<string> deploymentIds)
        {
            //这里没看明白activiti为什么抛出异常
            //if (deploymentIds == null)
            //{
            //    throw new ActivitiIllegalArgumentException("ids are null");
            //}
            if (deploymentIds == null)
            {
                this.deploymentIds_Renamed = null;
                return this;
            }
            this.deploymentIds_Renamed = deploymentIds;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionKey(string key)
        {
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(key, null))
            //{
            //    throw new ActivitiIllegalArgumentException("key is null");
            //}
            if (string.IsNullOrWhiteSpace(key))
            {
                this.key = null;
                return this;
            }
            this.key = key;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionKeys(ISet<string> keys)
        {
            //这里没看明白activiti为什么抛出异常
            //if (keys == null)
            //{
            //    throw new ActivitiIllegalArgumentException("keys is null");
            //}
            if (keys == null)
            {
                this.keys = null;
                return this;
            }
            this.keys = keys;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionKeyLike(string keyLike)
        {
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(keyLike, null))
            //{
            //    throw new ActivitiIllegalArgumentException("keyLike is null");
            //}
            if (string.IsNullOrWhiteSpace(keyLike))
            {
                this.keyLike = null;
                return this;
            }
            this.keyLike = keyLike;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionResourceName(string resourceName)
        {
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(resourceName, null))
            //{
            //    throw new ActivitiIllegalArgumentException("resourceName is null");
            //}
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                this.resourceName = null;
                return this;
            }
            this.resourceName = resourceName;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionResourceNameLike(string resourceNameLike)
        {
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(resourceNameLike, null))
            //{
            //    throw new ActivitiIllegalArgumentException("resourceNameLike is null");
            //}
            if (string.IsNullOrWhiteSpace(resourceNameLike))
            {
                this.resourceNameLike = null;
                return this;
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
            //这里没看明白activiti为什么抛出异常,允许从业务角度讲可以理解,但是从程序角度讲有点限制死了.
            //if (version == null)
            //{
            //    throw new ActivitiIllegalArgumentException("version is null");
            //}
            //else if (version <= 0)
            //{
            //    throw new ActivitiIllegalArgumentException("version must be positive");
            //}
        }

        public virtual IProcessDefinitionQuery latestVersion()
        {
            this.latest = true;
            return this;
        }

        public virtual IProcessDefinitionQuery active()
        {
            this.suspensionState = SuspensionStateProvider.ACTIVE;
            return this;
        }

        public virtual IProcessDefinitionQuery suspended()
        {
            this.suspensionState = SuspensionStateProvider.SUSPENDED;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionTenantId(string tenantId)
        {
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(tenantId, null))
            //{
            //    throw new ActivitiIllegalArgumentException("processDefinition tenantId is null");
            //}
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                this.tenantId = null;
                return this;
            }
            this.tenantId = tenantId;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionTenantIdLike(string tenantIdLike)
        {
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(tenantIdLike, null))
            //{
            //    throw new ActivitiIllegalArgumentException("process definition tenantId is null");
            //}
            if (string.IsNullOrWhiteSpace(tenantIdLike))
            {
                this.tenantIdLike = null;
                return this;
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
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(eventName, null))
            //{
            //    throw new ActivitiIllegalArgumentException("event name is null");
            //}
            //if (ReferenceEquals(eventType, null))
            //{
            //    throw new ActivitiException("event type is null");
            //}
            if (string.IsNullOrWhiteSpace(eventType) || string.IsNullOrWhiteSpace(eventName))
            {
                this.eventSubscriptionType = null;
                this.eventSubscriptionName = null;
                return this;
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
                if (!ReferenceEquals(authorizationUserId, null))
                {
                    IUserGroupLookupProxy userGroupLookupProxy = Context.ProcessEngineConfiguration.UserGroupLookupProxy;
                    if (userGroupLookupProxy != null)
                    {
                        return userGroupLookupProxy.getGroupsForCandidateUser(authorizationUserId);
                    }
                    else
                    {
                        log.LogWarning("No UserGroupLookupProxy set on ProcessEngineConfiguration. Tasks queried only where user is directly related, not through groups.");
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

        public virtual IProcessDefinitionQuery processDefinitionBusinessKey(string businessKey)
        {
            this. businessKey = businessKey;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionBusinessPath(string businessPath)
        {
            this.businessPath = businessPath;
            return this;
        }

        public virtual IProcessDefinitionQuery processDefinitionStartForm(string startForm)
        {
            this.startForm = startForm;
            return this;
        }

        public virtual string BusinessKey
        {
            get => businessKey;
            set => processDefinitionBusinessKey(value);
        }

        public virtual string BusinessPath
        {
            get => businessPath;
            set => processDefinitionBusinessPath(value);
        }

        public virtual string StartForm
        {
            get => startForm;
            set => processDefinitionStartForm(value);
        }

        // getters ////////////////////////////////////////////

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId_Renamed;
            }
            set => deploymentId_Renamed = value;
        }

        public virtual ISet<string> DeploymentIds
        {
            get
            {
                return deploymentIds_Renamed;
            }
            set => deploymentIds_Renamed = value;
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
            set => id = value;
        }

        public virtual ISet<string> Ids
        {
            get
            {
                return ids;
            }
            set => ids = value;
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
            set => name = value;
        }

        public virtual string NameLike
        {
            get
            {
                return nameLike;
            }
            set => nameLike = value;
        }

        public virtual string Key
        {
            get
            {
                return key;
            }
            set => key = value;
        }

        public virtual string KeyLike
        {
            get
            {
                return keyLike;
            }
            set => keyLike = value;
        }

        public virtual ISet<string> Keys
        {
            get
            {
                return keys;
            }
            set => keys = value;
        }

        public virtual int? Version
        {
            get
            {
                return version;
            }
            set => processDefinitionVersion(value);
        }

        public virtual int? VersionGt
        {
            get
            {
                return versionGt;
            }
            set => processDefinitionVersionGreaterThan(value);
        }

        public virtual int? VersionGte
        {
            get
            {
                return versionGte;
            }
            set => processDefinitionVersionGreaterThanOrEquals(value);
        }

        public virtual int? VersionLt
        {
            get
            {
                return versionLt;
            }
            set => processDefinitionVersionLowerThan(value);
        }

        public virtual int? VersionLte
        {
            get
            {
                return versionLte;
            }
            set => processDefinitionVersionLowerThanOrEquals(value);
        }

        public virtual bool Latest
        {
            get
            {
                return latest;
            }
            set
            {
                latest = value;
                if (value == true)
                {
                    latestVersion();
                }
            }
        }

        public virtual string Category
        {
            get
            {
                return category;
            }
            set => processDefinitionCategory(value);
        }

        public virtual string CategoryLike
        {
            get
            {
                return categoryLike;
            }
            set => processDefinitionCategoryLike(value);
        }

        public virtual string ResourceName
        {
            get
            {
                return resourceName;
            }
            set => processDefinitionResourceName(value);
        }

        public virtual string ResourceNameLike
        {
            get
            {
                return resourceNameLike;
            }
            set => processDefinitionResourceNameLike(value);
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
            set => processDefinitionCategoryNotEquals(value);
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set => processDefinitionTenantId(value);
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
            set => processDefinitionTenantIdLike(value);
        }

        public virtual bool WithoutTenantId
        {
            get
            {
                return withoutTenantId;
            }
            set
            {
                if (value == true)
                {
                    processDefinitionWithoutTenantId();
                }
            }
        }

        public virtual string AuthorizationUserId
        {
            get
            {
                return authorizationUserId;
            }
            set => startableByUser(value);
        }

        public virtual string ProcDefId
        {
            get
            {
                return procDefId;
            }
            set => processDefinitionStarter(value);
        }

        public virtual string EventSubscriptionName
        {
            get
            {
                return eventSubscriptionName;
            }
            set => eventSubscriptionName = value;
        }

        public virtual string EventSubscriptionType
        {
            get
            {
                return eventSubscriptionType;
            }
            set => EventSubscriptionType = value;
        }

        public virtual IProcessDefinitionQuery startableByUser(string userId)
        {
            //这里没看明白activiti为什么抛出异常
            //if (ReferenceEquals(userId, null))
            //{
            //    throw new ActivitiIllegalArgumentException("userId is null");
            //}
            if (string.IsNullOrWhiteSpace(userId))
            {
                this.authorizationUserId = null;
                return this;
            }
            this.authorizationUserId = userId;
            return this;
        }
    }

}