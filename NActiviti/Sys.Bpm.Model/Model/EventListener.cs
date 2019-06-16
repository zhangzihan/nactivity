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
namespace Sys.Workflow.bpmn.model
{
    /// <summary>
    /// Element for defining an event listener to hook in to the global event-mechanism.
    /// </summary>
    public class EventListener : BaseElement
    {

        protected internal string events;
        protected internal string implementationType;
        protected internal string implementation;
        protected internal string entityType;

        public virtual string Events
        {
            get
            {
                return events;
            }
            set
            {
                this.events = value;
            }
        }


        public virtual string ImplementationType
        {
            get
            {
                return implementationType;
            }
            set
            {
                this.implementationType = value;
            }
        }


        public virtual string Implementation
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


        public virtual string EntityType
        {
            set
            {
                this.entityType = value;
            }
            get
            {
                return entityType;
            }
        }


        public override BaseElement Clone()
        {
            EventListener clone = new EventListener
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                var val = value as EventListener;

                Events = val.Events;
                Implementation = val.Implementation;
                ImplementationType = val.ImplementationType;
                EntityType = val.EntityType;
            }
        }
    }

}