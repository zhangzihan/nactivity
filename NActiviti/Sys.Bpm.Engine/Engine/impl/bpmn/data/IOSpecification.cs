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
namespace Sys.Workflow.Engine.Impl.Bpmn.Datas
{

    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Implementation of the BPMN 2.0 'ioSpecification'
    /// 
    /// 
    /// 
    /// </summary>
    public class IOSpecification
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal IList<Data> dataInputs;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<Data> dataOutputs;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<DataRef> dataInputRefs;

        /// <summary>
        /// 
        /// </summary>
        protected internal IList<DataRef> dataOutputRefs;

        /// <summary>
        /// 
        /// </summary>
        public IOSpecification()
        {
            this.dataInputs = new List<Data>();
            this.dataOutputs = new List<Data>();
            this.dataInputRefs = new List<DataRef>();
            this.dataOutputRefs = new List<DataRef>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public virtual void Initialize(IExecutionEntity execution)
        {
            foreach (Data data in this.dataInputs)
            {
                execution.SetVariable(data.Name, data.Definition.CreateInstance());
            }

            foreach (Data data in this.dataOutputs)
            {
                execution.SetVariable(data.Name, data.Definition.CreateInstance());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<Data> DataInputs
        {
            get
            {
                return new ReadOnlyCollection<Data>(this.dataInputs);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<Data> DataOutputs
        {
            get
            {
                return new ReadOnlyCollection<Data>(dataOutputs);//Collections.unmodifiableList(this.dataOutputs);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddInput(Data data)
        {
            this.dataInputs.Add(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public virtual void AddOutput(Data data)
        {
            this.dataOutputs.Add(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataRef"></param>
        public virtual void AddInputRef(DataRef dataRef)
        {
            this.dataInputRefs.Add(dataRef);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataRef"></param>
        public virtual void AddOutputRef(DataRef dataRef)
        {
            this.dataOutputRefs.Add(dataRef);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string FirstDataInputName
        {
            get
            {
                return this.dataInputs[0].Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string FirstDataOutputName
        {
            get
            {
                if (this.dataOutputs is object && this.dataOutputs.Count > 0)
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