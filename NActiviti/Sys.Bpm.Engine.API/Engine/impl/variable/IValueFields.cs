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
    /// Common interface for regular and historic variable entities.
    /// 
    /// 
    /// </summary>
    public interface IValueFields
    {

        /// <returns> the name of the variable </returns>
        string Name { get; }

        /// <returns> the process instance id of the variable </returns>
        string ProcessInstanceId { get; }

        /// <returns> the execution id of the variable </returns>
        string ExecutionId { get; }

        /// <returns> the task id of the variable </returns>
        string TaskId { get; }

        /// <returns> the first text value, if any, or null. </returns>
        string TextValue { get; set; }


        /// <returns> the second text value, if any, or null. </returns>
        string TextValue2 { get; set; }


        /// <returns> the long value, if any, or null. </returns>
        long? LongValue { get; set; }


        /// <returns> the double value, if any, or null. </returns>
        double? DoubleValue { get; set; }


        /// <returns> the byte array value, if any, or null. </returns>
        byte[] Bytes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        object CachedValue { get; set; }
    }
}