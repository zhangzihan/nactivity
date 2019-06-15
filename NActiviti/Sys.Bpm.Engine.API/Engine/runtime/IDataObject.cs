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

namespace org.activiti.engine.runtime
{
    /// <summary>
    /// Represents a modeled DataObject.
    /// </summary>
    public interface IDataObject
    {
        /// <summary>
        /// Name of the DataObject.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Localized Name of the DataObject.
        /// </summary>
        string LocalizedName { get; }

        /// <summary>
        /// Description of the DataObject.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Value of the DataObject.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Type of the DataObject.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// The id of the flow element in the process defining this data object.
        /// </summary>
        string DataObjectDefinitionKey { get; }
    }
}