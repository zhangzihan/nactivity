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
namespace Sys.Workflow.Engine.Delegate.Events
{
    /// <summary>
    /// Describes a class that listens for <seealso cref="IActivitiEvent"/>s dispatched by the engine.
    /// 
    /// 
    /// </summary>
    public interface IActivitiEventListener
    {

        /// <summary>
        /// Called when an event has been fired
        /// </summary>
        /// <param name="event">
        ///          the event </param>
        void OnEvent(IActivitiEvent @event);

        /// <returns> whether or not the current operation should fail when this listeners execution throws an exception. </returns>
        bool FailOnException { get; }
    }
}