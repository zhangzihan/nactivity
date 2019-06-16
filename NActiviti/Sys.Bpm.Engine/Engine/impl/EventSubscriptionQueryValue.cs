using System;

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

namespace Sys.Workflow.Engine.Impl
{

    /// 
    [Serializable]
    public class EventSubscriptionQueryValue
    {

        private const long serialVersionUID = 1L;

        protected internal string eventType;
        protected internal string eventName;

        public EventSubscriptionQueryValue(string eventName, string eventType)
        {
            this.eventName = eventName;
            this.eventType = eventType;
        }

        public virtual string EventType
        {
            get
            {
                return eventType;
            }
            set
            {
                this.eventType = value;
            }
        }


        public virtual string EventName
        {
            get
            {
                return eventName;
            }
            set
            {
                this.eventName = value;
            }
        }


    }

}