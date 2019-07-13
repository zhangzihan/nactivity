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
namespace Sys.Workflow.Engine.Delegate.Events.Impl
{

    /// <summary>
    /// Base class for all <seealso cref="IActivitiEvent"/> implementations, related to entities.
    /// 
    /// 
    /// </summary>
    public class ActivitiTaskReturnToEventImpl : ActivitiEventImpl, IActivitiEntityEvent
    {
        private readonly object entity;
        private readonly string activityId;

        public ActivitiTaskReturnToEventImpl(object entity, ActivitiEventType type, string activityId) : base(type)
        {
            if (entity == null)
            {
                throw new ActivitiIllegalArgumentException("Entity cannot be null.");
            }
            this.entity = entity;
            this.activityId = activityId;
        }

        public virtual object Entity
        {
            get
            {
                return entity;
            }
        }

        public string ActivityId
        {
            get => activityId;
        }
    }

}