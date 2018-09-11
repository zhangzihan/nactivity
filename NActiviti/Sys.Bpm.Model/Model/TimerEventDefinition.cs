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
namespace org.activiti.bpmn.model
{
    public class TimerEventDefinition : EventDefinition
    {

        protected internal string timeDate;
        protected internal string timeDuration;
        protected internal string timeCycle;
        protected internal string endDate;
        protected internal string calendarName;

        public virtual string TimeDate
        {
            get
            {
                return timeDate;
            }
            set
            {
                this.timeDate = value;
            }
        }


        public virtual string TimeDuration
        {
            get
            {
                return timeDuration;
            }
            set
            {
                this.timeDuration = value;
            }
        }


        public virtual string TimeCycle
        {
            get
            {
                return timeCycle;
            }
            set
            {
                this.timeCycle = value;
            }
        }


        public virtual string EndDate
        {
            set
            {
                this.endDate = value;
            }
            get
            {
                return endDate;
            }
        }


        public virtual string CalendarName
        {
            get
            {
                return calendarName;
            }
            set
            {
                this.calendarName = value;
            }
        }


        public override BaseElement clone()
        {
            TimerEventDefinition clone = new TimerEventDefinition();
            clone.Values = this;
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as TimerEventDefinition;
                TimeDate = val.TimeDate;
                TimeDuration = val.TimeDuration;
                TimeCycle = val.TimeCycle;
                EndDate = val.EndDate;
                CalendarName = val.CalendarName;
            }
        }
    }

}