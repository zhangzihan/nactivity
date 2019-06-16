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
    /// An <seealso cref="Sys.Workflow.Engine.Delegate.Events.ActivitiCancelledEvent"/> implementation.
    /// 
    /// 
    /// </summary>
    public class ActivitiProcessCancelledEventImpl : ActivitiEventImpl, IActivitiCancelledEvent
    {

        protected internal object cause;

        public ActivitiProcessCancelledEventImpl() : base(ActivitiEventType.PROCESS_CANCELLED)
        {
        }

        public virtual object Cause
        {
            set
            {
                this.cause = value;
            }
            get
            {
                return cause;
            }
        }


    }

}