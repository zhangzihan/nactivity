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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    using Newtonsoft.Json;
    using Sys.Workflow.Engine.Impl.Bpmn.Datas;
    using Sys.Workflow.Engine.Impl.Contexts;

    /// 
    /// 
    [Serializable]
    public class ProcessDefinitionEntityImpl : AbstractEntity, IProcessDefinitionEntity
    {

        private const long serialVersionUID = 1L;

        protected internal string name;
        protected internal string description;
        protected internal string key;
        protected internal string businessKey;
        protected internal string businessPath;
        protected internal string startForm;
        protected internal int version;
        protected internal string category;
        protected internal string deploymentId;
        protected internal string resourceName;
        protected internal string tenantId = ProcessEngineConfiguration.NO_TENANT_ID;
        protected internal int? historyLevel;
        protected internal string diagramResourceName;
        protected internal bool isGraphicalNotationDefined;
        protected internal IDictionary<string, object> variables;
        protected internal bool hasStartFormKey;
        protected internal int suspensionState = SuspensionStateProvider.ACTIVE.StateCode;
        protected internal bool isIdentityLinksInitialized;
        protected internal IList<IIdentityLinkEntity> definitionIdentityLinkEntities = new List<IIdentityLinkEntity>();
        protected internal IOSpecification ioSpecification;

        // Backwards compatibility
        protected internal string engineVersion;

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState
                {
                    ["suspensionState"] = this.suspensionState,
                    ["category"] = this.category
                };
                return persistentState;
            }
        }

        // getters and setters
        // //////////////////////////////////////////////////////
        [JsonIgnore]
        public virtual IList<IIdentityLinkEntity> IdentityLinks
        {
            get
            {
                var ctx = Context.CommandContext;
                if (!isIdentityLinksInitialized && ctx != null)
                {
                    definitionIdentityLinkEntities = ctx.IdentityLinkEntityManager.FindIdentityLinksByProcessDefinitionId(id);
                    isIdentityLinksInitialized = true;
                }

                return definitionIdentityLinkEntities;
            }
        }

        public virtual string Key
        {
            get
            {
                return key;
            }
            set
            {
                this.key = value;
            }
        }

        public virtual string BusinessKey
        {
            get => businessKey;
            set => businessKey = value;
        }

        public virtual string BusinessPath
        {
            get => businessPath;
            set => businessPath = value;
        }

        public virtual string StartForm
        {
            get => startForm;
            set => startForm = value;
        }


        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }


        public virtual string Description
        {
            set
            {
                this.description = value;
            }
            get
            {
                return description;
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


        public virtual int Version
        {
            get
            {
                return version;
            }
            set
            {
                this.version = value;
            }
        }


        public virtual string ResourceName
        {
            get
            {
                return resourceName;
            }
            set
            {
                this.resourceName = value;
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


        public virtual int? HistoryLevel
        {
            get
            {
                return historyLevel;
            }
            set
            {
                this.historyLevel = value;
            }
        }


        public virtual IDictionary<string, object> Variables
        {
            get
            {
                return variables;
            }
            set
            {
                this.variables = value;
            }
        }


        public virtual string Category
        {
            get
            {
                return category;
            }
            set
            {
                this.category = value;
            }
        }


        public virtual string DiagramResourceName
        {
            get
            {
                return diagramResourceName;
            }
            set
            {
                this.diagramResourceName = value;
            }
        }

        public virtual bool HasStartFormKey
        {
            get
            {
                return hasStartFormKey;
            }
            set
            {
                this.hasStartFormKey = value;
            }
        }


        public virtual bool IsGraphicalNotationDefined
        {
            get
            {
                return isGraphicalNotationDefined;
            }
            set
            {
                this.isGraphicalNotationDefined = value;
            }
        }


        public virtual int SuspensionState
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


        public virtual bool Suspended
        {
            get
            {
                return suspensionState == SuspensionStateProvider.SUSPENDED.StateCode;
            }
        }

        public virtual string EngineVersion
        {
            get
            {
                return engineVersion;
            }
            set
            {
                this.engineVersion = value;
            }
        }


        public virtual IOSpecification IoSpecification
        {
            get
            {
                return ioSpecification;
            }
            set
            {
                this.ioSpecification = value;
            }
        }


        public override string ToString()
        {
            return "ProcessDefinitionEntity[" + id + "]";
        }

        public override bool Equals(object obj)
        {
            return this == obj as ProcessDefinitionEntityImpl;
        }

        public override int GetHashCode()
        {
            return this.GetType().GetHashCode() >> 2;
        }

        public static bool operator ==(ProcessDefinitionEntityImpl objA, ProcessDefinitionEntityImpl objB)
        {
            if (objA is null && objB is null)
            {
                return true;
            }

            if ((objA is object && objB is null) || (objB is object && objA is null))
            {
                return false;
            }

            return string.Equals(objA.Name, objB.Name, StringComparison.OrdinalIgnoreCase) && objA.Version == objB.Version && string.Equals(objA.TenantId, objB.TenantId, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(ProcessDefinitionEntityImpl objA, ProcessDefinitionEntityImpl objB)
        {
            return !(objA == objB);
        }
    }

}