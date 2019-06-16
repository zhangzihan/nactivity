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
using System;
using System.Collections.Concurrent;

namespace Sys.Workflow.Engine.Impl.Bpmn.Webservice
{

    /// <summary>
    /// An Operation is part of an <seealso cref="BpmnInterface"/> and it defines Messages that are consumed and (optionally) produced when the Operation is called.
    /// 
    /// 
    /// </summary>
    public class Operation
    {

        protected internal string id;

        protected internal string name;

        protected internal MessageDefinition inMessage;

        protected internal MessageDefinition outMessage;

        protected internal IOperationImplementation implementation;

        /// <summary>
        /// The interface to which this operations belongs
        /// </summary>
        protected internal BpmnInterface bpmnInterface;

        public Operation()
        {

        }

        public Operation(string id, string name, BpmnInterface bpmnInterface, MessageDefinition inMessage)
        {
            Id = id;
            Name = name;
            Interface = bpmnInterface;
            InMessage = inMessage;
        }

        public virtual MessageInstance SendMessage(MessageInstance message, ConcurrentDictionary<string, Uri> overridenEndpointAddresses)
        {
            return this.implementation.SendFor(message, this, overridenEndpointAddresses);
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


        public virtual BpmnInterface Interface
        {
            get
            {
                return bpmnInterface;
            }
            set
            {
                this.bpmnInterface = value;
            }
        }


        public virtual MessageDefinition InMessage
        {
            get
            {
                return inMessage;
            }
            set
            {
                this.inMessage = value;
            }
        }


        public virtual MessageDefinition OutMessage
        {
            get
            {
                return outMessage;
            }
            set
            {
                this.outMessage = value;
            }
        }


        public virtual IOperationImplementation Implementation
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