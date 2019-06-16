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
namespace Sys.Workflow.bpmn.model
{

    public abstract class Event : FlowNode
    {

        protected internal IList<EventDefinition> eventDefinitions = new List<EventDefinition>();

        public virtual IList<EventDefinition> EventDefinitions
        {
            get
            {
                return eventDefinitions;
            }
            set
            {
                this.eventDefinitions = value ?? new List<EventDefinition>();
            }
        }


        public virtual void AddEventDefinition(EventDefinition eventDefinition)
        {
            eventDefinitions.Add(eventDefinition);
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;

                var val = value as Event;

                eventDefinitions = new List<EventDefinition>();
                if (val.EventDefinitions != null && val.EventDefinitions.Count > 0)
                {
                    foreach (EventDefinition eventDef in val.EventDefinitions)
                    {
                        eventDefinitions.Add(eventDef.Clone() as EventDefinition);
                    }
                }
            }
        }
    }

}