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

namespace org.activiti.engine.task
{

    /// <summary>
    /// An identity link is used to associate a task with a certain identity.
    /// 
    /// For example: - a user can be an assignee (= identity link type) for a task - a group can be a candidate-group (= identity link type) for a task
    /// 
    /// 
    /// </summary>
    public interface IIdentityLink
    {

        /// <summary>
        /// Returns the type of link. See <seealso cref="IdentityLinkType"/> for the native supported types by Activiti.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// If the identity link involves a user, then this will be a non-null id of a user. That userId can be used to query for user information through the <seealso cref="UserQuery"/> API.
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// If the identity link involves a group, then this will be a non-null id of a group. That groupId can be used to query for user information through the <seealso cref="GroupQuery"/> API.
        /// </summary>
        string GroupId { get; }

        /// <summary>
        /// The id of the task associated with this identity link.
        /// </summary>
        string TaskId { get; }

        /// <summary>
        /// The process definition id associated with this identity link.
        /// </summary>
        string ProcessDefinitionId { get; }

        /// <summary>
        /// The process instance id associated with this identity link.
        /// </summary>
        string ProcessInstanceId { get; }
    }
}