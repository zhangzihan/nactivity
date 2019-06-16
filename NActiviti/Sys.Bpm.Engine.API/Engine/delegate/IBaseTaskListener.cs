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

namespace Sys.Workflow.engine.@delegate
{

    /// 
    public interface IBaseTaskListener
    {

        /// <summary>
        /// Not an actual event, used as a marker-value for <seealso cref="IBaseTaskListener"/>s that should be called for all events, including <seealso cref="EVENTNAME_CREATE"/> , <seealso cref="EVENTNAME_ASSIGNMENT"/> and
        /// <seealso cref="EVENTNAME_COMPLETE"/> and <seealso cref="EVENTNAME_DELETE"/>.
        /// </summary>
    }

    /// <summary>
    /// 
    /// </summary>
    public static class BaseTaskListenerFields
    {
        /// <summary>
        /// 
        /// </summary>
        public const string EVENTNAME_CREATE = "create";

        /// <summary>
        /// 
        /// </summary>
        public const string EVENTNAME_ASSIGNMENT = "assignment";

        /// <summary>
        /// 
        /// </summary>
        public const string EVENTNAME_COMPLETE = "complete";

        /// <summary>
        /// 
        /// </summary>
        public const string EVENTNAME_DELETE = "delete";

        /// <summary>
        /// 
        /// </summary>
        public const string EVENTNAME_ALL_EVENTS = "all";
    }
}