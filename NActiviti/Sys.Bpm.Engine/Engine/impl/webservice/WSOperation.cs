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
namespace Sys.Workflow.Engine.Impl.Webservice
{

    using Sys.Workflow.Engine.Impl.Bpmn.Webservice;
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Represents a WS implementation of a <seealso cref="Operation"/>
    /// </summary>
    public class WSOperation : IOperationImplementation
    {

        protected internal string id;

        protected internal string name;

        protected internal WSService service;

        public WSOperation(string id, string operationName, WSService service)
        {
            this.id = id;
            this.name = operationName;
            this.service = service;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual string Id
        {
            get
            {
                return this.id;
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual MessageInstance SendFor(MessageInstance message, Operation operation, ConcurrentDictionary<string, Uri> overridenEndpointAddresses)
        {
            object[] arguments = this.getArguments(message);
            object[] results = this.safeSend(arguments, overridenEndpointAddresses);
            return this.createResponseMessage(results, operation);
        }

        private object[] getArguments(MessageInstance message)
        {
            return message.StructureInstance.ToArray();
        }

        private object[] safeSend(object[] arguments, ConcurrentDictionary<string, Uri> overridenEndpointAddresses)
        {
            object[] results = null;

            results = this.service.Client.send(this.name, arguments, overridenEndpointAddresses);

            if (results is null)
            {
                results = new object[] { };
            }
            return results;
        }

        private MessageInstance createResponseMessage(object[] results, Operation operation)
        {
            MessageInstance message = null;
            MessageDefinition outMessage = operation.OutMessage;
            if (outMessage is not null)
            {
                message = outMessage.CreateInstance();
                message.StructureInstance.LoadFrom(results);
            }
            return message;
        }

        public virtual WSService Service
        {
            get
            {
                return this.service;
            }
        }
    }

}