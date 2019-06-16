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

namespace Sys.Workflow.Engine.Impl.Variable
{
    /// <summary>
    /// Interface describing a container for all available <seealso cref="IVariableType"/>s of variables.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IVariableTypes
    {

        /// <returns> the type for the given type name. Returns null if no type was found with the name. </returns>
        IVariableType GetVariableType(string typeName);

        /// <returns> the variable type to be used to store the given value as a variable. </returns>
        /// <exception cref="ActivitiException">
        ///           When no available type is capable of storing the value. </exception>
        IVariableType FindVariableType(object value);

        IVariableTypes AddType(IVariableType type);

        /// <summary>
        /// Add type at the given index. The index is used when finding a type for an object. When different types can store a specific object value, the one with the smallest index will be used.
        /// </summary>
        IVariableTypes AddType(IVariableType type, int index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        int GetTypeIndex(IVariableType type);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        int GetTypeIndex(string typeName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IVariableTypes RemoveType(IVariableType type);
    }
}