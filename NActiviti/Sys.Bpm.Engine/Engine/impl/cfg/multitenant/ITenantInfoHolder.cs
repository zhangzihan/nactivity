using System.Collections.Generic;

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
namespace Sys.Workflow.engine.impl.cfg.multitenant
{
    /// <summary>
    /// Interface to be implemented when using the <seealso cref="MultiSchemaMultiTenantProcessEngineConfiguration"/> and used 
    /// to set/get the current user and tenant identifier. 
    /// 
    /// The engine will call the <seealso cref="#getCurrentTenantId()"/> method when it needs to know which database to use.
    /// 
    /// Typically used with <seealso cref="ThreadLocal"/>'s in the implementation.
    /// 
    /// 
    /// </summary>
    public interface ITenantInfoHolder
    {

        /// <summary>
        /// Returns all known tenant identifiers.
        /// </summary>
        ICollection<string> AllTenants { get; }

        /// <summary>
        /// Sets the current tenant identifier.
        /// </summary>
        string CurrentTenantId { set; get; }


        /// <summary>
        /// Clears the current tenant identifier settings.
        /// </summary>
        void ClearCurrentTenantId();
    }
}