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
namespace Sys.Workflow.engine.impl.cmd
{
    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.impl.interceptor;

    /// <summary>
    /// Command that dispatches an event.
    /// 
    /// 
    /// </summary>
    public class DispatchEventCommand : ICommand<object>
    {

        protected internal IActivitiEvent @event;

        public DispatchEventCommand(IActivitiEvent @event)
        {
            this.@event = @event;
        }

        public  virtual object  Execute(ICommandContext commandContext)
        {
            if (@event == null)
            {
                throw new ActivitiIllegalArgumentException("event is null");
            }

            if (commandContext.EventDispatcher.Enabled)
            {
                commandContext.EventDispatcher.DispatchEvent(@event);
            }
            else
            {
                throw new ActivitiException("Message dispatcher is disabled, cannot dispatch event");
            }

            return null;
        }

    }

}