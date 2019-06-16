using Newtonsoft.Json;
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
namespace Sys.Workflow.Bpmn.Models
{
    public abstract class FlowElement : BaseElement, IHasExecutionListeners
    {
        protected internal string name;
        protected internal string documentation;
        protected internal IList<ActivitiListener> executionListeners = new List<ActivitiListener>();

        [JsonIgnore]
        protected internal IFlowElementsContainer parentContainer;

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


        public virtual string Documentation
        {
            get
            {
                return documentation;
            }
            set
            {
                this.documentation = value;
            }
        }


        public virtual IList<ActivitiListener> ExecutionListeners
        {
            get
            {
                return executionListeners;
            }
            set
            {
                this.executionListeners = value;
            }
        }

        [JsonIgnore]
        public virtual IFlowElementsContainer ParentContainer
        {
            get
            {
                return parentContainer;
            }
            set
            {
                this.parentContainer = value;
            }
        }

        [JsonIgnore]
        public virtual SubProcess SubProcess
        {
            get
            {
                SubProcess subProcess = null;
                if (parentContainer is SubProcess)
                {
                    subProcess = (SubProcess)parentContainer;
                }

                return subProcess;
            }
        }


        public override abstract BaseElement Clone();

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as FlowElement;

                Name = val.Name;
                Documentation = val.Documentation;

                executionListeners = new List<ActivitiListener>();
                if (val.ExecutionListeners != null && val.ExecutionListeners.Count > 0)
                {
                    foreach (ActivitiListener listener in val.ExecutionListeners)
                    {
                        executionListeners.Add(listener.Clone() as ActivitiListener);
                    }
                }
            }
        }
    }

}