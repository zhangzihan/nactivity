﻿/* Licensed under the Apache License, Version 2.0 (the "License");
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
namespace Sys.Workflow.engine.impl.bpmn.webservice
{
    using Sys.Workflow.engine.impl.bpmn.data;

    /// <summary>
    /// An instance of a <seealso cref="MessageDefinition"/>
    /// 
    /// 
    /// </summary>
    public class MessageInstance
    {

        protected internal MessageDefinition message;

        protected internal ItemInstance item;

        public MessageInstance(MessageDefinition message, ItemInstance item)
        {
            this.message = message;
            this.item = item;
        }

        public virtual IStructureInstance StructureInstance
        {
            get
            {
                return this.item.StructureInstance;
            }
        }

        public virtual MessageDefinition Message
        {
            get
            {
                return this.message;
            }
        }
    }

}