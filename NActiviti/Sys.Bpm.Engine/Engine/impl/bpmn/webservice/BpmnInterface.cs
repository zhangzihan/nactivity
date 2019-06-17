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
namespace Sys.Workflow.Engine.Impl.Bpmn.Webservice
{

    /// <summary>
    /// An Interface defines a set of operations that are implemented by services external to the process.
    /// 
    /// 
    /// </summary>
    public class BpmnInterface
    {

        protected internal string id;

        protected internal string name;

        protected internal IBpmnInterfaceImplementation implementation;

        /// <summary>
        /// Mapping of the operations of this interface. The key of the map is the id of the operation, for easy retrieval.
        /// </summary>
        protected internal IDictionary<string, Operation> operations = new Dictionary<string, Operation>();

        public BpmnInterface()
        {

        }

        public BpmnInterface(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
            set
            {
                this.id = value;
            }
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


        public virtual void AddOperation(Operation operation)
        {
            operations[operation.Id] = operation;
        }

        public virtual Operation GetOperation(string operationId)
        {
            operations.TryGetValue(operationId, out var operation);

            return operation;
        }

        public virtual ICollection<Operation> Operations
        {
            get
            {
                return operations.Values;
            }
        }

        public virtual IBpmnInterfaceImplementation Implementation
        {
            get
            {
                return implementation;
            }
            set
            {
                this.implementation = value;
            }
        }

    }

}