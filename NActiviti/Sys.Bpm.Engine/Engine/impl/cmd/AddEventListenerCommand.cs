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
namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Impl.Interceptor;

    /// <summary>
    /// Command that adds an event-listener to the Activiti engine.
    /// 
    /// 
    /// </summary>
    public class AddEventListenerCommand : ICommand<object>
    {

        protected internal IActivitiEventListener listener;
        protected internal ActivitiEventType[] types;

        public AddEventListenerCommand(IActivitiEventListener listener, ActivitiEventType[] types)
        {
            this.listener = listener;
            this.types = types;
        }

        public AddEventListenerCommand(IActivitiEventListener listener) : base()
        {
            this.listener = listener;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            if (listener is null)
            {
                throw new ActivitiIllegalArgumentException("listener is null.");
            }

            if (types is not null)
            {
                commandContext.ProcessEngineConfiguration.EventDispatcher.AddEventListener(listener, types);
            }
            else
            {
                commandContext.ProcessEngineConfiguration.EventDispatcher.AddEventListener(listener);
            }

            return null;
        }

    }

}