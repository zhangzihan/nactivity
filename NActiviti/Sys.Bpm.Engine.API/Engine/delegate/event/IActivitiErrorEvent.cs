﻿/* Licensed under the Apache License, Version 2.0 (the "License");
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
    /// An <seealso cref="IActivitiEvent"/> related to an error being sent to an activity.
    /// 
    /// 
    /// </summary>
    public interface IActivitiErrorEvent : IActivitiActivityEvent
    {

        /// <returns> the error-code of the error. Returns null, if no specific error-code has been specified when the error was thrown. </returns>
        string ErrorCode { get; }

        string ErrorId { get; }

    }

}