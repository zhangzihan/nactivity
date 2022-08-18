using System;
using System.Collections.Generic;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.DB;
    using Sys.Net.Http;
    using Sys.Workflow;

    /// 
    /// 
    /// 
    [Serializable]
    public class HistoricProcessInstanceEntityImpl : HistoricScopeInstanceEntityImpl, IHistoricProcessInstanceEntity, IBulkDeleteable
    {

        private const long serialVersionUID = 1L;

        private string endActivityId;
        private string businessKey;
        private string startUserId;
        private string startActivityId;
        private string superProcessInstanceId;
        private string tenantId = ProcessEngineConfiguration.NO_TENANT_ID;
        private string name;
        private string localizedName;
        private string description;
        private string localizedDescription;
        private string processDefinitionKey;
        private string processDefinitionName;
        private int? processDefinitionVersion;
        private string deploymentId;
        private IList<IHistoricVariableInstanceEntity> queryVariables;

        public HistoricProcessInstanceEntityImpl()
        {

        }

        public HistoricProcessInstanceEntityImpl(IExecutionEntity processInstance)
        {
            id = processInstance.Id;
            processInstanceId = processInstance.Id;
            businessKey = processInstance.BusinessKey;
            processDefinitionId = processInstance.ProcessDefinitionId;
            processDefinitionKey = processInstance.ProcessDefinitionKey;
            processDefinitionName = processInstance.ProcessDefinitionName;
            processDefinitionVersion = processInstance.ProcessDefinitionVersion;
            deploymentId = processInstance.DeploymentId;
            startTime = processInstance.StartTime;
            startUserId = processInstance.StartUserId;
            startActivityId = processInstance.ActivityId;
            superProcessInstanceId = processInstance.SuperExecution?.ProcessInstanceId;

            // Inherit tenant id (if applicable)
            if (processInstance.TenantId is not null)
            {
                tenantId = processInstance.TenantId;
            }
        }

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState
                {
                    ["endTime"] = endTime,
                    ["businessKey"] = businessKey,
                    ["name"] = name,
                    ["durationInMillis"] = durationInMillis,
                    ["deleteReason"] = deleteReason,
                    ["endStateName"] = endActivityId,
                    ["superProcessInstanceId"] = superProcessInstanceId,
                    ["processDefinitionId"] = processDefinitionId,
                    ["processDefinitionKey"] = processDefinitionKey,
                    ["processDefinitionName"] = processDefinitionName,
                    ["processDefinitionVersion"] = processDefinitionVersion,
                    ["deploymentId"] = deploymentId
                };
                return persistentState;
            }
        }

        // getters and setters ////////////////////////////////////////////////////////

        public virtual string EndActivityId
        {
            get
            {
                return endActivityId;
            }
            set
            {
                this.endActivityId = value;
            }
        }


        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
            set
            {
                this.businessKey = string.Empty.Equals(value?.Trim()) ? null : value?.Trim();
            }
        }


        public virtual string StartUserId
        {
            get
            {
                return startUserId;
            }
            set
            {
                this.startUserId = string.Empty.Equals(value?.Trim()) ? null : value?.Trim();
            }
        }

        public virtual string StartUser
        {
            get; set;
        }

        internal static IEnumerable<IHistoricProcessInstanceEntity> EnsureStarterInitialized(IEnumerable<HistoricProcessInstanceEntityImpl> insts)
        {
            foreach (var inst in insts ?? new HistoricProcessInstanceEntityImpl[0])
            {
                inst.EnsureStarterInitialized();

                yield return inst;
            }

            yield break;
        }

        private IUserInfo EnsureStarterInitialized()
        {
            if (StartUser is not null)
            {
                starter = new UserInfo
                {
                    Id = startUserId,
                    FullName = StartUser
                };
                return starter;
            }

            if (Context.CommandContext is object && (starter is null || starter.Id != this.startUserId))
            {
                if (this.ProcessVariables.TryGetValue(this.startUserId, out var userInfo) && userInfo is not null)
                {
                    starter = JToken.FromObject(userInfo).ToObject<UserInfo>();

                    return starter;
                }
            }

            starter = new UserInfo
            {
                Id = startUserId
            };

            return starter;
        }

        private IUserInfo starter = null;

        public virtual IUserInfo Starter
        {
            get
            {
                if (starter is null)
                {
                    starter = EnsureStarterInitialized();
                }

                return starter;
                //                throw new Sys.Workflow.Engine.Exceptions.NotFoundAssigneeException(this.assignee);  
                //#endif
            }
        }


        public virtual string StartActivityId
        {
            get
            {
                return startActivityId;
            }
            set
            {
                this.startActivityId = value;
            }
        }


        public virtual string SuperProcessInstanceId
        {
            get
            {
                return superProcessInstanceId;
            }
            set
            {
                this.superProcessInstanceId = value;
            }
        }


        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set
            {
                this.tenantId = value;
            }
        }


        public virtual string Name
        {
            get
            {
                if (localizedName is not null && localizedName.Length > 0)
                {
                    return localizedName;
                }
                else
                {
                    return name;
                }
            }
            set
            {
                this.name = value;
            }
        }


        public virtual string LocalizedName
        {
            get
            {
                return localizedName;
            }
            set
            {
                this.localizedName = value;
            }
        }


        public virtual string Description
        {
            get
            {
                if (localizedDescription is not null && localizedDescription.Length > 0)
                {
                    return localizedDescription;
                }
                else
                {
                    return description;
                }
            }
            set
            {
                this.description = value;
            }
        }


        public virtual string LocalizedDescription
        {
            get
            {
                return localizedDescription;
            }
            set
            {
                this.localizedDescription = value;
            }
        }


        public virtual string ProcessDefinitionKey
        {
            get
            {
                return processDefinitionKey;
            }
            set
            {
                this.processDefinitionKey = value;
            }
        }


        public virtual string ProcessDefinitionName
        {
            get
            {
                return processDefinitionName;
            }
            set
            {
                this.processDefinitionName = value;
            }
        }


        public virtual int? ProcessDefinitionVersion
        {
            get
            {
                return processDefinitionVersion;
            }
            set
            {
                this.processDefinitionVersion = value;
            }
        }


        public virtual string DeploymentId
        {
            get
            {
                return deploymentId;
            }
            set
            {
                this.deploymentId = value;
            }
        }


        public virtual IDictionary<string, object> ProcessVariables
        {
            get
            {
                IDictionary<string, object> variables = new Dictionary<string, object>();
                if (queryVariables is object)
                {
                    foreach (IHistoricVariableInstanceEntity variableInstance in queryVariables)
                    {
                        if (variableInstance.Id is not null && variableInstance.TaskId is null)
                        {
                            variables[variableInstance.Name] = variableInstance.Value;
                        }
                    }
                }
                return variables;
            }
        }

        public virtual IList<IHistoricVariableInstanceEntity> QueryVariables
        {
            get
            {
                if (queryVariables is null && Context.CommandContext is object)
                {
                    queryVariables = new HistoricVariableInitializingList();
                }
                return queryVariables;
            }
            set
            {
                this.queryVariables = value;
            }
        }


        // common methods //////////////////////////////////////////////////////////

        public override string ToString()
        {
            return "HistoricProcessInstanceEntity[superProcessInstanceId=" + superProcessInstanceId + "]";
        }
    }

}