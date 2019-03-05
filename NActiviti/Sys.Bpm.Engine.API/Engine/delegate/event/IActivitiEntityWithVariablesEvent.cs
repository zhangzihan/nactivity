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
using System.Collections.Generic;

namespace org.activiti.engine.@delegate.@event
{

    /// <summary>
    /// An <seealso cref="IActivitiEntityEvent"/> related to a single entity.
    /// 
    /// 
    /// </summary>
    public interface IActivitiEntityWithVariablesEvent : IActivitiEntityEvent
    {
        /// <returns> the variables created together with the entity. </returns>
        IDictionary<string, object> Variables { get; }

        bool LocalScope { get; }
    }

}