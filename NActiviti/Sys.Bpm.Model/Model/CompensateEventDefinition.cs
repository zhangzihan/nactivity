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
    public class CompensateEventDefinition : EventDefinition
    {

        protected internal string activityRef;
        protected internal bool waitForCompletion = true;

        public virtual string ActivityRef
        {
            get
            {
                return activityRef;
            }
            set
            {
                this.activityRef = value;
            }
        }


        public virtual bool WaitForCompletion
        {
            get
            {
                return waitForCompletion;
            }
            set
            {
                this.waitForCompletion = value;
            }
        }


        public override BaseElement clone()
        {
            CompensateEventDefinition clone = new CompensateEventDefinition();
            clone.Values = this;
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as CompensateEventDefinition;
                ActivityRef = val.ActivityRef;
                WaitForCompletion = val.WaitForCompletion;
            }
        }
    }

}