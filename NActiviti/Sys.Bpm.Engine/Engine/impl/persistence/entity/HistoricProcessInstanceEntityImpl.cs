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

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.db;

    /// 
    /// 
    /// 
    [Serializable]
    public class HistoricProcessInstanceEntityImpl : HistoricScopeInstanceEntityImpl, IHistoricProcessInstanceEntity, IBulkDeleteable
    {

        private const long serialVersionUID = 1L;

        protected internal string endActivityId;
        protected internal string businessKey;
        protected internal string startUserId;
        protected internal string startActivityId;
        protected internal string superProcessInstanceId;
        protected internal string tenantId = ProcessEngineConfiguration.NO_TENANT_ID;
        protected internal string name;
        protected internal string localizedName;
        protected internal string description;
        protected internal string localizedDescription;
        protected internal string processDefinitionKey;
        protected internal string processDefinitionName;
        protected internal int? processDefinitionVersion;
        protected internal string deploymentId;
        protected internal IList<IHistoricVariableInstanceEntity> queryVariables;

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
            superProcessInstanceId = processInstance.SuperExecution != null ? processInstance.SuperExecution.ProcessInstanceId : null;

            // Inherit tenant id (if applicable)
            if (!ReferenceEquals(processInstance.TenantId, null))
            {
                tenantId = processInstance.TenantId;
            }
        }

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState();
                persistentState["endTime"] = endTime;
                persistentState["businessKey"] = businessKey;
                persistentState["name"] = name;
                persistentState["durationInMillis"] = durationInMillis;
                persistentState["deleteReason"] = deleteReason;
                persistentState["endStateName"] = endActivityId;
                persistentState["superProcessInstanceId"] = superProcessInstanceId;
                persistentState["processDefinitionId"] = processDefinitionId;
                persistentState["processDefinitionKey"] = processDefinitionKey;
                persistentState["processDefinitionName"] = processDefinitionName;
                persistentState["processDefinitionVersion"] = processDefinitionVersion;
                persistentState["deploymentId"] = deploymentId;
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
                this.businessKey = value;
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
                this.startUserId = value;
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
                if (!ReferenceEquals(localizedName, null) && localizedName.Length > 0)
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
                if (!ReferenceEquals(localizedDescription, null) && localizedDescription.Length > 0)
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
                if (queryVariables != null)
                {
                    foreach (IHistoricVariableInstanceEntity variableInstance in queryVariables)
                    {
                        if (!ReferenceEquals(variableInstance.Id, null) && ReferenceEquals(variableInstance.TaskId, null))
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
                if (queryVariables == null && Context.CommandContext != null)
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