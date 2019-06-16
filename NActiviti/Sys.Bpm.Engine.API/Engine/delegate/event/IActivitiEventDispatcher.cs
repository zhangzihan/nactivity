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
namespace Sys.Workflow.engine.@delegate.@event
{
    /// <summary>
    /// Dispatcher which allows for adding and removing <seealso cref="IActivitiEventListener"/> s to the Activiti Engine as well as dispatching <seealso cref="IActivitiEvent"/> to all the listeners registered.
    /// 
    /// 
    /// </summary>
    public interface IActivitiEventDispatcher
    {
        /// <summary>
        /// Adds an event-listener which will be notified of ALL events by the dispatcher.
        /// </summary>
        /// <param name="listenerToAdd">
        ///          the listener to add </param>
        void AddEventListener(IActivitiEventListener listenerToAdd);

        /// <summary>
        /// Adds an event-listener which will only be notified when an event of the given types occurs.
        /// </summary>
        /// <param name="listenerToAdd">
        ///          the listener to add </param>
        /// <param name="types">
        ///          types of events the listener should be notified for </param>
        void AddEventListener(IActivitiEventListener listenerToAdd, params ActivitiEventType[] types);

        /// <summary>
        /// Removes the given listener from this dispatcher. The listener will no longer be notified, regardless of the type(s) it was registered for in the first place.
        /// </summary>
        /// <param name="listenerToRemove">
        ///          listener to remove </param>
        void RemoveEventListener(IActivitiEventListener listenerToRemove);

        /// <summary>
        /// Dispatches the given event to any listeners that are registered.
        /// </summary>
        /// <param name="event">
        ///          event to dispatch. </param>
        void DispatchEvent(IActivitiEvent @event);

        /// <summary>
        /// true, if event dispatching should be enabled.
        /// </summary>
        bool Enabled { set; get; }

    }

}