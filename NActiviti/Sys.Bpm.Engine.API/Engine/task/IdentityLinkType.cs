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
namespace Sys.Workflow.engine.task
{

    /// <summary>
    /// Contains constants for all types of identity links that can be used to involve a user or group with a certain task.
    /// </summary>
    /// <seealso cref= ITaskService.AddUserIdentityLink(String, String, String) </seealso>
    /// <seealso cref= TaskService#addGroupIdentityLink(String, String, String)
    /// 
    ///  </seealso>
    public class IdentityLinkType
    {

        /* Activiti native roles */
        /// <summary>
        /// 
        /// </summary>
        public const string ASSIGNEE = "assignee";

        /// <summary>
        /// 
        /// </summary>
        public const string CANDIDATE = "candidate";

        /// <summary>
        /// 
        /// </summary>
        public const string OWNER = "owner";

        /// <summary>
        /// 
        /// </summary>
        public const string STARTER = "starter";

        /// <summary>
        /// 
        /// </summary>
        public const string PARTICIPANT = "participant";
    }
}