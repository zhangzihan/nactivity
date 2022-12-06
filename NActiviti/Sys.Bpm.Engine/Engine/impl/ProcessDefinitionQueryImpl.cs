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
	using Microsoft.Extensions.Logging;
	using Sys.Workflow.Engine.Impl.Contexts;
	using Sys.Workflow.Engine.Impl.Interceptor;
	using Sys.Workflow.Engine.Impl.Persistence.Entity;
	using Sys.Workflow.Engine.Repository;
	using Sys.Workflow;

	[Serializable]
	public class ProcessDefinitionQueryImpl : AbstractQuery<IProcessDefinitionQuery, IProcessDefinition>, IProcessDefinitionQuery
	{
		private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<ProcessDefinitionQueryImpl>();

		private const long serialVersionUID = 1L;

		private string id;
		private string[] ids;
		private string category;
		private string categoryLike;
		private string categoryNotEquals;
		private string businessKey;
		private string businessPath;
		private string startForm;
		private string name;
		private string nameLike;
		private string _deploymentId;
		private string[] _deploymentIds;
		private string key;
		private string keyLike;
		private string[] keys;
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

		public virtual IProcessDefinitionQuery SetProcessDefinitionId(string processDefinitionId)
		{
			this.id = processDefinitionId;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionIds(string[] processDefinitionIds)
		{
			this.ids = processDefinitionIds;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionCategory(string category)
		{
			if (string.IsNullOrWhiteSpace(category))
			{
				this.category = null;
				return this;
			}
			this.category = category;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionCategoryLike(string categoryLike)
		{
			if (string.IsNullOrWhiteSpace(categoryLike))
			{
				this.categoryLike = null;
				return this;
			}
			this.categoryLike = categoryLike;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionCategoryNotEquals(string categoryNotEquals)
		{
			if (string.IsNullOrWhiteSpace(categoryNotEquals))
			{
				this.categoryNotEquals = null;
				return this;
			}
			this.categoryNotEquals = categoryNotEquals;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				this.name = null;
				return this;
			}
			this.name = name;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionNameLike(string nameLike)
		{
			if (string.IsNullOrWhiteSpace(nameLike))
			{
				this.nameLike = null;
				return this;
			}
			this.nameLike = nameLike;
			return this;
		}

		public virtual IProcessDefinitionQuery SetDeploymentId(string deploymentId)
		{
			if (string.IsNullOrWhiteSpace(deploymentId))
			{
				this._deploymentId = null;
				return this;
			}
			this._deploymentId = deploymentId;
			return this;
		}

		public virtual IProcessDefinitionQuery SetDeploymentIds(string[] deploymentIds)
		{
			if (deploymentIds is null)
			{
				this._deploymentIds = null;
				return this;
			}
			this._deploymentIds = deploymentIds;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionKey(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				this.key = null;
				return this;
			}
			this.key = key;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionKeys(string[] keys)
		{
			if (keys is null)
			{
				this.keys = null;
				return this;
			}
			this.keys = keys;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionKeyLike(string keyLike)
		{
			if (string.IsNullOrWhiteSpace(keyLike))
			{
				this.keyLike = null;
				return this;
			}
			this.keyLike = keyLike;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionResourceName(string resourceName)
		{
			if (string.IsNullOrWhiteSpace(resourceName))
			{
				this.resourceName = null;
				return this;
			}
			this.resourceName = resourceName;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionResourceNameLike(string resourceNameLike)
		{
			if (string.IsNullOrWhiteSpace(resourceNameLike))
			{
				this.resourceNameLike = null;
				return this;
			}
			this.resourceNameLike = resourceNameLike;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionVersion(int? version)
		{
			CheckVersion(version);
			this.version = version;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionVersionGreaterThan(int? processDefinitionVersion)
		{
			CheckVersion(processDefinitionVersion);
			this.versionGt = processDefinitionVersion;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionVersionGreaterThanOrEquals(int? processDefinitionVersion)
		{
			CheckVersion(processDefinitionVersion);
			this.versionGte = processDefinitionVersion;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionVersionLowerThan(int? processDefinitionVersion)
		{
			CheckVersion(processDefinitionVersion);
			this.versionLt = processDefinitionVersion;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionVersionLowerThanOrEquals(int? processDefinitionVersion)
		{
			CheckVersion(processDefinitionVersion);
			this.versionLte = processDefinitionVersion;
			return this;
		}

		protected internal virtual void CheckVersion(int? version)
		{
			//if (version <= 0)
			//{
			//    throw new ActivitiIllegalArgumentException("version must be positive");
			//}
		}

		public virtual IProcessDefinitionQuery SetLatestVersion()
		{
			this.SetProcessDefinitionVersion(null);
			this.SetProcessDefinitionVersionGreaterThan(null);
			this.SetProcessDefinitionVersionGreaterThanOrEquals(null);
			this.SetProcessDefinitionVersionLowerThan(null);
			this.SetProcessDefinitionVersionLowerThanOrEquals(null);
			this.latest = true;
			return this;
		}

		public virtual IProcessDefinitionQuery SetActive()
		{
			this.suspensionState = SuspensionStateProvider.ACTIVE;
			return this;
		}

		public virtual IProcessDefinitionQuery SetSuspended()
		{
			this.suspensionState = SuspensionStateProvider.SUSPENDED;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionTenantId(string tenantId)
		{
			this.tenantId = tenantId?.Trim();
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionTenantIdLike(string tenantIdLike)
		{
			if (string.IsNullOrWhiteSpace(tenantIdLike))
			{
				this.tenantIdLike = null;
				return this;
			}

			this.tenantIdLike = tenantIdLike;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionWithoutTenantId()
		{
			this.withoutTenantId = true;
			return this;
		}

		public virtual IProcessDefinitionQuery SetMessageEventSubscription(string messageName)
		{
			return SetEventSubscription("message", messageName);
		}

		public virtual IProcessDefinitionQuery SetMessageEventSubscriptionName(string messageName)
		{
			return SetEventSubscription("message", messageName);
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionStarter(string procDefId)
		{
			this.procDefId = procDefId;
			return this;
		}

		public virtual IProcessDefinitionQuery SetEventSubscription(string eventType, string eventName)
		{
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
				if (authorizationUserId is not null)
				{
					IUserGroupLookupProxy userGroupLookupProxy = Context.ProcessEngineConfiguration.UserGroupLookupProxy;
					if (userGroupLookupProxy is object)
					{
						return userGroupLookupProxy.GetGroupsForCandidateUser(authorizationUserId);
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

		public virtual IProcessDefinitionQuery OrderByDeploymentId()
		{
			return SetOrderBy(ProcessDefinitionQueryProperty.DEPLOYMENT_ID);
		}

		public virtual IProcessDefinitionQuery OrderByProcessDefinitionKey()
		{
			return SetOrderBy(ProcessDefinitionQueryProperty.PROCESS_DEFINITION_KEY);
		}

		public virtual IProcessDefinitionQuery OrderByProcessDefinitionCategory()
		{
			return SetOrderBy(ProcessDefinitionQueryProperty.PROCESS_DEFINITION_CATEGORY);
		}

		public virtual IProcessDefinitionQuery OrderByProcessDefinitionId()
		{
			return SetOrderBy(ProcessDefinitionQueryProperty.PROCESS_DEFINITION_ID);
		}

		public virtual IProcessDefinitionQuery OrderByProcessDefinitionVersion()
		{
			return SetOrderBy(ProcessDefinitionQueryProperty.PROCESS_DEFINITION_VERSION);
		}

		public virtual IProcessDefinitionQuery OrderByProcessDefinitionName()
		{
			return SetOrderBy(ProcessDefinitionQueryProperty.PROCESS_DEFINITION_NAME);
		}

		public virtual IProcessDefinitionQuery OrderByTenantId()
		{
			return SetOrderBy(ProcessDefinitionQueryProperty.PROCESS_DEFINITION_TENANT_ID);
		}

		// results ////////////////////////////////////////////

		public override long ExecuteCount(ICommandContext commandContext)
		{
			CheckQueryOk();

			return commandContext.ProcessDefinitionEntityManager.FindProcessDefinitionCountByQueryCriteria(this);
		}

		public override IList<IProcessDefinition> ExecuteList(ICommandContext commandContext, Page page)
		{
			CheckQueryOk();

			return commandContext.ProcessDefinitionEntityManager.FindProcessDefinitionsByQueryCriteria(this, page);
		}

		protected internal override void CheckQueryOk()
		{
			base.CheckQueryOk();
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionBusinessKey(string businessKey)
		{
			this.businessKey = businessKey;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionBusinessPath(string businessPath)
		{
			this.businessPath = businessPath;
			return this;
		}

		public virtual IProcessDefinitionQuery SetProcessDefinitionStartForm(string startForm)
		{
			this.startForm = startForm;
			return this;
		}

		public virtual string BusinessKey
		{
			get => businessKey;
			set => SetProcessDefinitionBusinessKey(value);
		}

		public virtual string BusinessPath
		{
			get => businessPath;
			set => SetProcessDefinitionBusinessPath(value);
		}

		public virtual string StartForm
		{
			get => startForm;
			set => SetProcessDefinitionStartForm(value);
		}

		// getters ////////////////////////////////////////////

		public virtual string DeploymentId
		{
			get
			{
				return _deploymentId;
			}
			set => _deploymentId = value;
		}

		public virtual string[] DeploymentIds
		{
			get
			{
				return _deploymentIds;
			}
			set => _deploymentIds = value;
		}

		public virtual string Id
		{
			get
			{
				return id;
			}
			set => id = value;
		}

		public virtual string[] Ids
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

		public virtual string[] Keys
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
			set => SetProcessDefinitionVersion(value);
		}

		public virtual int? VersionGt
		{
			get
			{
				return versionGt;
			}
			set => SetProcessDefinitionVersionGreaterThan(value);
		}

		public virtual int? VersionGte
		{
			get
			{
				return versionGte;
			}
			set => SetProcessDefinitionVersionGreaterThanOrEquals(value);
		}

		public virtual int? VersionLt
		{
			get
			{
				return versionLt;
			}
			set => SetProcessDefinitionVersionLowerThan(value);
		}

		public virtual int? VersionLte
		{
			get
			{
				return versionLte;
			}
			set => SetProcessDefinitionVersionLowerThanOrEquals(value);
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
					SetLatestVersion();
				}
			}
		}

		public virtual string Category
		{
			get
			{
				return category;
			}
			set => SetProcessDefinitionCategory(value);
		}

		public virtual string CategoryLike
		{
			get
			{
				return categoryLike;
			}
			set => SetProcessDefinitionCategoryLike(value);
		}

		public virtual string ResourceName
		{
			get
			{
				return resourceName;
			}
			set => SetProcessDefinitionResourceName(value);
		}

		public virtual string ResourceNameLike
		{
			get
			{
				return resourceNameLike;
			}
			set => SetProcessDefinitionResourceNameLike(value);
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
			set => SetProcessDefinitionCategoryNotEquals(value);
		}

		public virtual string TenantId
		{
			get
			{
				return tenantId;
			}
			set => SetProcessDefinitionTenantId(value);
		}

		public virtual string TenantIdLike
		{
			get
			{
				return tenantIdLike;
			}
			set => SetProcessDefinitionTenantIdLike(value);
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
					SetProcessDefinitionWithoutTenantId();
				}
			}
		}

		public virtual string AuthorizationUserId
		{
			get
			{
				return authorizationUserId;
			}
			set => SetStartableByUser(value);
		}

		public virtual string ProcDefId
		{
			get
			{
				return procDefId;
			}
			set => SetProcessDefinitionStarter(value);
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

		public virtual IProcessDefinitionQuery SetStartableByUser(string userId)
		{
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