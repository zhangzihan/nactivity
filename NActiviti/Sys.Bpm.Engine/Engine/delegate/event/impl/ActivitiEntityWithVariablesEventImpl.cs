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

namespace Sys.Workflow.Engine.Delegate.Events.Impl
{


    /// <summary>
    /// Base class for all <seealso cref="IActivitiEntityEvent"/> implementations, related to entities with variables.
    /// 
    /// 
    /// </summary>
    public class ActivitiEntityWithVariablesEventImpl : ActivitiEntityEventImpl, IActivitiEntityWithVariablesEvent
    {

        protected internal IDictionary<string, object> variables;
        protected internal bool localScope;

        public ActivitiEntityWithVariablesEventImpl(object entity, IDictionary<string, object> variables, bool localScope, ActivitiEventType type) : base(entity, type)
        {

            this.variables = variables;
            this.localScope = localScope;
        }

        public virtual IDictionary<string, object> Variables
        {
            get
            {
                return variables;
            }
        }

        public virtual bool LocalScope
        {
            get
            {
                return localScope;
            }
        }
    }

}