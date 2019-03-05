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
namespace org.activiti.engine.impl.persistence.entity
{
    public static class SuspensionStateProvider
    {
        public static readonly ISuspensionState ACTIVE = new SuspensionStateImpl(1, "active");
        public static readonly ISuspensionState SUSPENDED = new SuspensionStateImpl(2, "suspended");
    }
}