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
namespace org.activiti.engine.impl.bpmn.data
{

    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.persistence.entity;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Implementation of the BPMN 2.0 'ioSpecification'
    /// 
    /// 
    /// 
    /// </summary>
    public class IOSpecification
    {

        protected internal IList<Data> dataInputs;

        protected internal IList<Data> dataOutputs;

        protected internal IList<DataRef> dataInputRefs;

        protected internal IList<DataRef> dataOutputRefs;

        public IOSpecification()
        {
            this.dataInputs = new List<Data>();
            this.dataOutputs = new List<Data>();
            this.dataInputRefs = new List<DataRef>();
            this.dataOutputRefs = new List<DataRef>();
        }

        public virtual void initialize(IExecutionEntity execution)
        {
            foreach (Data data in this.dataInputs)
            {
                execution.setVariable(data.Name, data.Definition.createInstance());
            }

            foreach (Data data in this.dataOutputs)
            {
                execution.setVariable(data.Name, data.Definition.createInstance());
            }
        }

        public virtual IList<Data> DataInputs
        {
            get
            {
                return new ReadOnlyCollection<Data>(this.dataInputs);
            }
        }

        public virtual IList<Data> DataOutputs
        {
            get
            {
                return new ReadOnlyCollection<Data>(dataOutputs);//Collections.unmodifiableList(this.dataOutputs);
            }
        }

        public virtual void addInput(Data data)
        {
            this.dataInputs.Add(data);
        }

        public virtual void addOutput(Data data)
        {
            this.dataOutputs.Add(data);
        }

        public virtual void addInputRef(DataRef dataRef)
        {
            this.dataInputRefs.Add(dataRef);
        }

        public virtual void addOutputRef(DataRef dataRef)
        {
            this.dataOutputRefs.Add(dataRef);
        }

        public virtual string FirstDataInputName
        {
            get
            {
                return this.dataInputs[0].Name;
            }
        }

        public virtual string FirstDataOutputName
        {
            get
            {
                if (this.dataOutputs != null && this.dataOutputs.Count > 0)
                {
                    return this.dataOutputs[0].Name;
                }
                else
                {
                    return null;
                }
            }
        }
    }

}