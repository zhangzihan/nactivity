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
    /// Implementation of <seealso cref="IActivitiMembershipEvent"/>.
    /// 
    /// 
    /// </summary>
    public class ActivitiMembershipEventImpl : ActivitiEventImpl, IActivitiMembershipEvent
    {

        protected internal string userId;
        protected internal string groupId;

        public ActivitiMembershipEventImpl(ActivitiEventType type) : base(type)
        {
        }

        public virtual string UserId
        {
            set
            {
                this.userId = value;
            }
            get
            {
                return userId;
            }
        }


        public virtual string GroupId
        {
            set
            {
                this.groupId = value;
            }
            get
            {
                return groupId;
            }
        }

    }

}