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
namespace Sys.Workflow.engine.@delegate.@event.impl
{

    /// <summary>
    /// Implementation of an <seealso cref="IActivitiActivityEvent"/>.
    /// 
    /// 
    /// 
    /// </summary>
    public class ActivitiActivityEventImpl : ActivitiEventImpl, IActivitiActivityEvent
    {

        protected internal string activityId;
        protected internal string activityName;
        protected internal string activityType;
        protected internal string behaviorClass;

        public ActivitiActivityEventImpl(ActivitiEventType type) : base(type)
        {
        }

        public virtual string ActivityId
        {
            get
            {
                return activityId;
            }
            set
            {
                this.activityId = value;
            }
        }


        public virtual string ActivityName
        {
            get
            {
                return activityName;
            }
            set
            {
                this.activityName = value;
            }
        }


        public virtual string ActivityType
        {
            get
            {
                return activityType;
            }
            set
            {
                this.activityType = value;
            }
        }


        public virtual string BehaviorClass
        {
            get
            {
                return behaviorClass;
            }
            set
            {
                this.behaviorClass = value;
            }
        }


    }

}